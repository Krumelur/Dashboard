using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cronos;
using Dashboard.Harvester.DataSources;
using Dashboard.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Harvester
{
	public static class Helpers
	{
		public static SourceConfig LoadSourceConfig(string sourceId, ILogger logger)
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dashboardharvester");

			logger.LogInformation($"Loading source config for ID '{sourceId}' from '{appDataFolder}'");

			if (!Directory.Exists(appDataFolder))
			{
				logger.LogInformation($"Cannot find folder '{appDataFolder}' - creating it now.");
				Directory.CreateDirectory(appDataFolder);
			}

			var configFullPath = Path.Combine(appDataFolder, $"{sourceId}.json");
			if (!File.Exists(configFullPath))
			{
				logger.LogInformation($"Failed to find config file for ID '{sourceId}' - will create one based on template. Make sure to update it.");
				File.Copy("sourceconfigtemplate.json", configFullPath);
			}

			var jsonConfig = File.ReadAllText(Path.Combine(appDataFolder, $"{sourceId}.json"));
			var sourceConfig = JsonConvert.DeserializeObject<SourceConfig>(jsonConfig);

			return sourceConfig;
		}

		public static SourceConfig SaveSourceConfig(SourceConfig config, ILogger logger)
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dashboardharvester");

			if (!Directory.Exists(appDataFolder))
			{
				Directory.CreateDirectory(appDataFolder);
			}

			logger.LogInformation($"Saving source config for ID '{config.Id}' to '{appDataFolder}'");

			var jsonConfig = JsonConvert.SerializeObject(config);

			using var file = File.CreateText(Path.Combine(appDataFolder, $"{config.Id}.json"));
			file.Write(jsonConfig);

			return config;
		}

		public static async Task ProcessSource(SourceConfig sourceConfig, CosmosClient dbClient, HarvesterSettings harvesterSettings, IConfiguration configuration, ILogger logger)
		{
			logger.LogInformation($"Config entry to be processed: {sourceConfig}");

			var sourceTypeName = ("source" + sourceConfig.Id).ToLowerInvariant();
			var sourceType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.IsClass && t.Name.ToLowerInvariant() == sourceTypeName);
			if (sourceType == null)
			{
				logger.LogCritical($"Failed to find type for source ID '{sourceConfig.Id}'. Used '{sourceTypeName}' to scan for it.");
				return;
			}

			var source = (SourceBase)Activator.CreateInstance(sourceType, harvesterSettings, logger);
			if (source == null)
			{
				logger.LogCritical($"Failed to create instance of source for type '{sourceType}'");
				return;
			}

			SourceData sourceData = null;
			try
			{
				sourceData = await source.GetData();	
			}
			catch (Exception ex)
			{
				logger.LogCritical($"Error getting source data: {ex}");
			}
			

			// Update source configuration.
			sourceConfig.LastUpdateUtc = DateTimeOffset.UtcNow;
			var sourceConfigCronExpression = CronExpression.Parse(sourceConfig.CronExecutionTime);
			var lastUpdateUtc = sourceConfig.LastUpdateUtc != null ? sourceConfig.LastUpdateUtc.Value.UtcDateTime : DateTimeOffset.UtcNow.UtcDateTime;
			DateTime? nextExecutionUtc = sourceConfigCronExpression.GetNextOccurrence(lastUpdateUtc);
			sourceConfig.NextExecutionDueUtc = sourceConfigCronExpression.GetNextOccurrence(sourceConfig.LastUpdateUtc.Value.UtcDateTime);
			SaveSourceConfig(sourceConfig, logger);

			if(sourceData == null)
			{
				return;
			}

			// Save to database.
			if(dbClient != null)
			{
				var container = dbClient.GetContainer("dashboard", "sourcedata");
				await container.UpsertItemAsync(sourceData);
			}
			else
			{
				logger.LogWarning("CosmosClient is NULL - not saving source data!");
			}
		}
	}
}