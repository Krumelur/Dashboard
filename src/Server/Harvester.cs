using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using Flurl.Http;
using Dashboard.Models;
using Cronos;

namespace Dashboard.Server
{
	/// <summary>
	/// Collection of functions that help reading the configured sources and, poll them and
	/// write the results back into a history table.
	/// </summary>
	public class Harvester
    {
		public Harvester(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

		/// <summary>
		/// Timer triggered function.
		/// Its schedule is configured by the config parameter 'HarvesterSchdule' which
		/// must be a CRON expression.
		/// The function kicks off the harvesting processes of the sources.
		/// It is looking for a config parameter named 'HarvesterUrl' and it is then making a
		/// call to this URL.
		/// </summary>
		[FunctionName("TriggerSourceHarvesterByTimer")]
        public async Task TriggerSourceHarvesterByTimer(
			[TimerTrigger("%HarvesterSchedule%", RunOnStartup = false)] TimerInfo timerInfo,
            ILogger log)
        {
			// This URL should be pointing to the route of HarvestConfiguredSources().
			var harvesterUrl = _config["HarvesterUrl"];

			if(string.IsNullOrWhiteSpace(harvesterUrl))
			{
				log.LogError("Missing configuration parameter 'HarvesterUrl'. This must be set and point to the harvester logic.");
				return;
			}

			var extendedPermsFuncAuthKey = _config["ExtendedPermsFunctionsAuthKey"];
			if(String.IsNullOrWhiteSpace(extendedPermsFuncAuthKey))
			{
				log.LogError("Failed to retrieve harvester authorization key. Is it added to the configuration?");
				return;
			}

			var nextRun = timerInfo.Schedule.GetNextOccurrence(DateTime.UtcNow);
			log.LogInformation($"Starting harvester at URL {harvesterUrl}. Next occurrence will by at UTC {nextRun}");

			// var client = new HttpClient();
			// var r = await client.GetStringAsync(harvesterUrl);
			// Fire & forget. Read every source but don't wait.
			var processedSourceConfigs = new List<SourceConfig>();
			try
			{
				// Note: when trying to use a timer with RunsOnStartup = true", the request will fail.
				// I presume the functions runtime isn't done initializing and will throw an error stating
				// that the connection was actively refused.
				 processedSourceConfigs = await harvesterUrl
					.WithHeader("x-functions-key", extendedPermsFuncAuthKey)
					.WithTimeout(60)
					.GetJsonAsync<List<SourceConfig>>();

			}
			catch(FlurlHttpException ex)
			{
				log.LogError($"Failed to call harvester URL: {ex.Message}");
			}

			foreach (var item in processedSourceConfigs)
			{
				log.LogInformation($"Auto trigger processed: {item}");	
			}
		}

		/// <summary>
		/// HTTP triggered function (GET).
		/// 
		/// This is called from TriggerSourceHarvesterByTimer().
		/// 
		/// Reads source configurations from the collection 'sourceconfig' in the database 'dashboard'
		/// found at the Cosmos DB instance configured by the connection string 'CosmosDbConnectionString'.
		/// All enabled sources that are due for processing will be polled.
		/// Because of the execution time limits of functions, the completion of the polling will not be awaited.
		/// 
		/// Supported query parameters:
		///   ignoreLastUpdate (boolean): TRUE to process enabled sources even if the are not due
		///   awaitCompletion (boolean): TRUE to await every call to to the source specific harvesting - useful for debugging
		/// </summary>
		/// <returns>A list of source configuration entries that were processed</returns>
        [FunctionName("HarvestConfiguredSources")]
        public async Task<IActionResult> HarvestConfiguredSources(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "harvestconfiguredsources")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<SourceConfig> SourceConfigs,
            ILogger log)
        {
			log.LogInformation("Harvesting all sources");

			// This function should only run if the ExtendedPermsFunctionsAuthKey is passed in
			// via the x-functions-key header.
			var authKey = _config["ExtendedPermsFunctionsAuthKey"];
			if(!Helpers.CheckFunctionAuthKey(authKey, req))
			{
				return new BadRequestObjectResult("Failed to verify ExtendedPermsFunctionsAuthKey.");
			}

			var configItemsToProcess = new List<SourceConfig>();
			
			bool ignoreLastUpdate = false;
			if(req.Query.ContainsKey("ignoreLastUpdate"))
			{
				Boolean.TryParse(req.Query["ignoreLastUpdate"], out ignoreLastUpdate);
			}

			bool awaitCompletion = false;
			if(req.Query.ContainsKey("awaitCompletion"))
			{
				Boolean.TryParse(req.Query["awaitCompletion"], out awaitCompletion);
			}

			foreach (var SourceConfig in SourceConfigs)
			{
				// Process all sources that are enabled and are overdue.
				if(string.IsNullOrWhiteSpace(SourceConfig.CronExecutionTime))
				{
					log.LogWarning($"Skipping config entry. CRON execution time is null or whitespace: {SourceConfig}");
					continue;
				}

				var cronExpression = CronExpression.Parse(SourceConfig.CronExecutionTime);
				DateTime? nextExecutionUtc = cronExpression.GetNextOccurrence(SourceConfig.LastUpdateUtc.UtcDateTime);
			
				if(nextExecutionUtc == null)
				{
					log.LogWarning($"Skipping config entry. It doesn't have a valid CRON execution time: {SourceConfig}");
					continue;
				}

				if(!SourceConfig.IsEnabled)
				{
					log.LogWarning($"Skipping config entry. It's not enabled: {SourceConfig}");
					continue;
				}
				
				log.LogInformation($"Current UTC time is '{DateTimeOffset.UtcNow}'. Next execution time UTC is '{nextExecutionUtc}' for config entry: {SourceConfig}");
					
				if((ignoreLastUpdate || SourceConfig.LastUpdateUtc == DateTimeOffset.MinValue || nextExecutionUtc < DateTimeOffset.UtcNow))
				{
					log.LogInformation($"Config entry to be processed: {SourceConfig}");
					configItemsToProcess.Add(SourceConfig);
				}
				else
				{
					log.LogInformation($"Config entry skipped. It's not due: {SourceConfig}");
				}
			}

			foreach (var SourceConfig in configItemsToProcess)
			{
				// For every source, run the "PersistSource" function but don't wait for it to return.
				// Functions have a an execution timeout. If there are many sources or response is slow,
				// we'd risk getting terminated by the runtime.				
				var postUrl = req.Scheme + "://" + req.Host + "/api/harvestsource";

				log.LogInformation($"About to harvest source ID '{SourceConfig.Id}' ('{SourceConfig.Name}') using URL '{postUrl}'");
				
				try
				{
					// Fire & forget. Read every source but don't wait.
					var fireAndForgetTask = postUrl
						.WithHeader("x-functions-key", authKey)
						.WithTimeout(60)
						.PostJsonAsync(SourceConfig)
						.ReceiveJson<SourceConfig>();

					if(awaitCompletion)
					{
						log.LogInformation($"Awaiting completion of source ID '{SourceConfig.Id}' ('{SourceConfig.Name}')");
						var harvestedData = await fireAndForgetTask;

						log.LogInformation($"Received source config item: '{harvestedData}'");
					}
				}
				catch(Exception ex)
				{
					log.LogError($"Error while calling harvester for source ID '{SourceConfig.Id}' ('{SourceConfig.Name}') using URL '{postUrl}': {ex}");
				}
			}
			
			return new OkObjectResult(configItemsToProcess);
        }

		/// <summary>
		/// HTTP triggered function (POST).
		/// Reads a source, awaits the call and stores the result data into a Cosmos DB instance 
		/// configured by the connection string 'CosmosDbConnectionString', using the collection 'sourcedata'
		/// in the database named 'dashboard'. Upon succesful processing, it updates the collection 'sourceconfig'
		/// by setting the source configurations 'LastUpdateUtc' property to the processing time.		
		/// 
		/// The source to be processed must be passed in the request body as a JSON representation of
		/// a SourceConfig instance. 
		/// </summary>
		/// <returns>An object with two properties, the updated source configuration item and the history item</returns>
		[FunctionName("HarvestAndPersistSource")]
        public async Task<IActionResult> HarvestAndPersistSource(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "harvestsource")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<SourceConfig> updatedSourceConfigs,
           	[CosmosDB("dashboard", "sourcedata", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<SourceData> sourceDataItems,
           	ILogger log)
        {
			log.LogInformation($"Harvesting specific source");

			// This function should only run if the ExtendedPermsFunctionsAuthKey is passed in
			// via the x-functions-key header.
			var authKey = _config["ExtendedPermsFunctionsAuthKey"];
			if(!Helpers.CheckFunctionAuthKey(authKey, req))
			{
				return new BadRequestObjectResult("Failed to verify ExtendedPermsFunctionsAuthKey.");
			}

			// Get source config item from the request's body.
			SourceConfig SourceConfig = null;
			var content = await new StreamReader(req.Body).ReadToEndAsync();
			try
			{
				SourceConfig = JsonConvert.DeserializeObject<SourceConfig>(content);
				log.LogInformation(SourceConfig.ToString());
			}
			catch (System.Exception ex)
			{
				log.LogError($"Failed to deserialize source config data: {ex}");
				return new BadRequestObjectResult($"Failed to deserialize source config data: {ex}");
			}

			// Read the source.
			SourceData sourceData = null;
			try
			{
				sourceData = await SourceConfig.Url
					.WithHeader("x-functions-key", authKey)
					.WithTimeout(180)
					.GetJsonAsync<SourceData>();

				// Write back to storage with updated date/time.
				SourceConfig.LastUpdateUtc = DateTimeOffset.UtcNow;
				var cronExpression = CronExpression.Parse(SourceConfig.CronExecutionTime);
				SourceConfig.NextExecutionDueUtc = cronExpression.GetNextOccurrence(SourceConfig.LastUpdateUtc.UtcDateTime);
			}
			catch(Exception ex)
			{
				log.LogError($"Failed to get source data for entry {SourceConfig}: {ex}");
				return new BadRequestObjectResult($"Failed to get source data for entry {SourceConfig}: {ex}");
			}

			// Write source result to history table.
			var sourceDataItem = new SourceData
			{
				Id = Guid.NewGuid().ToString(),
				SourceId = sourceData.Id,
				DataItems = sourceData.DataItems,
				TimeStampUtc = sourceData.TimeStampUtc
			};
			
			await sourceDataItems.AddAsync(sourceDataItem);

			// Store the ID of the latest history item in the source config for easy access.
			SourceConfig.LatestHistoryItemId = sourceDataItem.Id;
			await updatedSourceConfigs.AddAsync(SourceConfig);

			return new OkObjectResult(new  {
				updatedSourceConfigEntry = SourceConfig,
				historyEntry = sourceDataItem
			});
        }
    }
	
}
