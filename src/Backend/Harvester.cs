using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Data;
using Flurl.Http;
using System.IO;
using Newtonsoft.Json;

namespace Functions
{
	public class Harvester
    {
		public Harvester(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

		[FunctionName("AutoTriggerSourceConfigProcessing")]
        public async Task AutoTriggerSourceConfigProcessing(
#if DEBUG
			[TimerTrigger("%HarvesterSchedule%", RunOnStartup = true)] TimerInfo timerInfo,
#else
			[TimerTrigger("%HarvesterSchedule%", RunOnStartup = false)] TimerInfo timerInfo,
#endif
            ILogger log)
        {
			var harvesterUrl = _config["HarvesterUrl"];

			var nextRun = timerInfo.Schedule.GetNextOccurrence(DateTime.UtcNow);
			log.LogInformation($"Starting harvester at URL {harvesterUrl}. Next occurrence will by at UTC {nextRun}");

				// Fire & forget. Read every source but don't wait.
			var processedSourceConfigItems = await harvesterUrl
					.WithTimeout(60)
					.GetJsonAsync<List<SourceConfigItem>>();

			foreach (var item in processedSourceConfigItems)
			{
				log.LogInformation($"Auto trigger processed: {item}");	
			}
		}

        [FunctionName("ProcessSourceConfig")]
        public IActionResult ProcessSourceConfig(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "harvest")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<SourceConfigItem> sourceConfigItems,
            ILogger log)
        {
			log.LogInformation("Harvesting all sources");

			var configItemsToProcess = new List<SourceConfigItem>();
			
			bool ignoreLastUpdate = false;
			if(req.Query.ContainsKey("ignoreLastUpdate"))
			{
				Boolean.TryParse(req.Query["ignoreLastUpdate"], out ignoreLastUpdate);
			}
			foreach (var sourceConfigItem in sourceConfigItems)
			{
				// Process all sources that are enabled and are overdue.
				if(sourceConfigItem.IsEnabled && (ignoreLastUpdate || sourceConfigItem.LastUpdateUtc == DateTimeOffset.MinValue || sourceConfigItem.LastUpdateUtc.AddMinutes(sourceConfigItem.IntervalMinutes) > DateTimeOffset.UtcNow))
				{
					log.LogInformation($"Config entry to be processed: {sourceConfigItem}");
					configItemsToProcess.Add(sourceConfigItem);
				}
				else
				{
					log.LogInformation($"Config entry skipped (not enabled or not due): {sourceConfigItem}");
				}
			}

			foreach (var sourceConfigItem in configItemsToProcess)
			{
				// For every source, run the "PersistSource" function but don't wait for it to return.
				// Functions have a an execution timeout. If there are many sources or response is slow,
				// we'd risk getting terminated by the runtime.				
				var postUrl = req.Scheme + "://" + req.Host + "/api/harvest";

				// Fire & forget. Read every source but don't wait.
				var fireAndForgetTask = postUrl
					.WithTimeout(60)
					.PostJsonAsync(sourceConfigItem)
					.ReceiveJson<SourceConfigItem>();
			}
			
			return new OkObjectResult(configItemsToProcess);
        }

		[FunctionName("PersistSource")]
        public async Task<IActionResult> PersistSource(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "harvest")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<SourceConfigItem> updatedSourceConfigItems,
           	[CosmosDB("dashboard", "sourcedatahistory", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<SourceDataHistoryItem> sourceDataHistoryItems,
           	ILogger log)
        {
			log.LogInformation($"Harvesting specific source");

			// Get source config item from the request's body.
			SourceConfigItem sourceConfigItem = null;
			var content = await new StreamReader(req.Body).ReadToEndAsync();
			try
			{
				sourceConfigItem = JsonConvert.DeserializeObject<SourceConfigItem>(content);
				log.LogInformation(sourceConfigItem.ToString());
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
				sourceData = await sourceConfigItem.Url
					.WithTimeout(180)
					.GetJsonAsync<SourceData>();

				// Write back to storage with updated date/time.
				sourceConfigItem.LastUpdateUtc = DateTimeOffset.UtcNow;
				await updatedSourceConfigItems.AddAsync(sourceConfigItem);
			}
			catch(Exception ex)
			{
				log.LogError($"Failed to get source data for entry {sourceConfigItem}: {ex}");
				return new BadRequestObjectResult($"Failed to get source data for entry {sourceConfigItem}: {ex}");
			}

			// Write source result to history table.
			var sourceDataHistoryItem = new SourceDataHistoryItem
			{
				Id = Guid.NewGuid().ToString(),
				SourceId = sourceData.Id,
				DataItems = sourceData.DataItems,
				TimeStampUtc = sourceData.TimeStampUtc
			};
			
			await sourceDataHistoryItems.AddAsync(sourceDataHistoryItem);

			return new OkObjectResult(sourceConfigItem);
        }
    }
	
}
