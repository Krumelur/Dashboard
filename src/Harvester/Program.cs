using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Cronos;
using Dashboard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using NReco.Logging.File;

namespace Harvester
{
	class Program
	{
		public static IConfigurationRoot Configuration { get; set; }

		public static ILogger Logger { get; set; }

		public static CosmosClient DbClient { get; set; }

		public static HarvesterSettings HarvesterSettings { get; set; }

		static async Task Main(string[] args)
		{
			CommandLineOptions commandLineOptions = null;
			Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed(opt => commandLineOptions = opt);

			//Determines the working environment as IHostingEnvironment is unavailable in a console app
			var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
			var isDevelopment = devEnvironmentVariable != null && devEnvironmentVariable.ToLower() == "development";

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
			
			// Build now because we need configuration from appsettings.json to build the configuration for key vault.
			Configuration = builder.Build();

			var loggingSection = Configuration.GetSection("Logging");
			
			var serviceProvider = new ServiceCollection()
				.AddLogging(cfg => {
					cfg.AddConsole();
					if(isDevelopment)
					{
						cfg.AddDebug();
					}
				})
				.AddLogging(cfg => cfg.AddFile(loggingSection))
				.Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Trace)
				.BuildServiceProvider();
			
			Logger = serviceProvider.GetService<ILogger<Program>>();

			HarvesterSettings = new HarvesterSettings();
			Configuration.Bind("HarvesterSettings", HarvesterSettings);

			if(commandLineOptions.IgnoreSecretSettings)
			{
				Logger.LogInformation("Ignoring user secrets and key vault settings - forcing usage of appsettings.json");
			}
			else
			{
				// Only add secrets in development - otherwise use key vault.
				if (isDevelopment)
				{
					Logger.LogInformation("Using local configuration settings");
					builder.AddUserSecrets<Program>();
					Configuration = builder.Build();
				}
				else
				{
					Logger.LogInformation("Using key vault's configuration settings");

					if(string.IsNullOrWhiteSpace(HarvesterSettings.KeyVaultUri))
					{
						Logger.LogError("Cannot launch harvester - 'KeyVaultUri' is missing from appsettings.json.");
						return;
					}

					if(string.IsNullOrWhiteSpace(commandLineOptions.KeyVaultClientId))
					{
						Logger.LogError("Cannot launch harvester - command line parameter 'keyvaultclientid' must be specified.");
						return;
					}

					if(string.IsNullOrWhiteSpace(commandLineOptions.KeyVaultClientSecret))
					{
						Logger.LogError("Cannot launch harvester - command line parameter 'keyvaultclientsecret' must be specified.");
						return;
					}

					// Use Azure Key Vault from a console app: https://c-sharx.net/read-secrets-from-azure-key-vault-in-a-net-core-console-app
					builder.AddAzureKeyVault(HarvesterSettings.KeyVaultUri, clientId: commandLineOptions.KeyVaultClientId, clientSecret: commandLineOptions.KeyVaultClientSecret);
					Configuration = builder.Build();
				}
			}

			// Bind again because values from user secrets or key vault have been added.
			// Important: recreate the settings or array-types will be duplicated.
			HarvesterSettings = new HarvesterSettings();
			Configuration.Bind("HarvesterSettings", HarvesterSettings);

			Console.WriteLine("Dashboard Harvester");
			Console.WriteLine();
			await RunHarvester(commandLineOptions);
		}

		static async Task RunHarvester(CommandLineOptions options)
		{
			Logger.LogInformation("Starting dashboard harvester");

			if (options.IgnoreLastSourceUpdate)
			{
				Logger.LogWarning("--> Ignoring last update time of sources!");
			}

			if (string.IsNullOrWhiteSpace(HarvesterSettings.HarvesterSchedule))
			{
				Logger.LogCritical("Failed getting harvester schedule setting. Have you added the 'HarvesterSchedule' CRON expression to the settings?");
				return;
			}
			else
			{
				Logger.LogInformation($"Harvester schedule cron expression: '{HarvesterSettings.HarvesterSchedule}'");
			}

			if (HarvesterSettings.ConfiguredSources == null || HarvesterSettings.ConfiguredSources.Length <= 0)
			{
				Logger.LogError("No sources configured - have you set 'ConfiguredSources' in appsettings.json?");
				return;
			}

			if (string.IsNullOrWhiteSpace(HarvesterSettings.CosmosDbConnectionString))
			{
				Logger.LogError("Cannot connect to database - have you set 'CosmosDbConnectionString' in settings (user secrets or key vault)?");
				return;
			}
			var connectionStringBuilder = new DbConnectionStringBuilder
			{
				ConnectionString = HarvesterSettings.CosmosDbConnectionString
			};

			connectionStringBuilder.TryGetValue("AccountKey", out object key);
			connectionStringBuilder.TryGetValue("AccountEndpoint", out object uri);
			DbClient = new CosmosClient(uri.ToString(), key.ToString());

			// Loop forever: check if a source is due, process it, sleep.
			while (true)
			{
				Logger.LogInformation("Processing sources...");

				foreach (var sourceId in HarvesterSettings.ConfiguredSources)
				{
					Logger.LogInformation($"Processing source ID '{sourceId}'");

					SourceConfig sourceConfig;
					try
					{
						sourceConfig = Helpers.LoadSourceConfig(sourceId, Logger);
					}
					catch (Exception ex)
					{
						Logger.LogCritical($"Skipping source - failed to load source config for 'solar': {ex}");
						continue;
					}

					var sourceConfigCronExpression = CronExpression.Parse(sourceConfig.CronExecutionTime);
					var lastUpdateUtc = sourceConfig.LastUpdateUtc != null ? sourceConfig.LastUpdateUtc.Value.UtcDateTime : DateTimeOffset.UtcNow.UtcDateTime;
					DateTime? nextExecutionUtc = sourceConfigCronExpression.GetNextOccurrence(lastUpdateUtc);

					if (nextExecutionUtc == null)
					{
						Logger.LogWarning($"Skipping config entry. It doesn't have a valid CRON execution time: {sourceConfig}");
						continue;
					}

					if (!sourceConfig.IsEnabled)
					{
						Logger.LogWarning($"Skipping config entry. It's not enabled: {sourceConfig}");
						continue;
					}

					Logger.LogInformation($"Current UTC time is '{DateTimeOffset.UtcNow}'. Next execution time UTC is '{nextExecutionUtc}' for config entry: {sourceConfig}");

					if (options.IgnoreLastSourceUpdate || sourceConfig.LastUpdateUtc == null || sourceConfig.LastUpdateUtc == DateTimeOffset.MinValue || nextExecutionUtc < DateTimeOffset.UtcNow)
					{
						await Helpers.ProcessSource(sourceConfig, DbClient, HarvesterSettings, Configuration, Logger);
					}
					else
					{
						Logger.LogInformation($"Config entry skipped. It's not due: {sourceConfig}");
					}
				}

				var harvesterCronExpression = CronExpression.Parse(HarvesterSettings.HarvesterSchedule);
				DateTime nextHarvesterExecutionUtc = harvesterCronExpression.GetNextOccurrence(DateTime.UtcNow).Value;

				var utcNow = DateTime.UtcNow;
				var hibernationTime = nextHarvesterExecutionUtc - utcNow;
				Logger.LogInformation($"Harvester sleeping. Went to sleep at UTC: '{utcNow}'; waking up for next execution at UTC '{nextHarvesterExecutionUtc}'.");
				await Task.Delay(hibernationTime);
			}
		}
	}
}
