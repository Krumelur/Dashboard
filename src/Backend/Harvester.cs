using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using DataItems;
using Flurl.Http;
using System.IO;
using Newtonsoft.Json;

namespace Functions
{
	[StorageAccount("StorageAccountConnectionString")]
	public class Harvester
    {
		public Harvester(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

        [FunctionName("ProcessSourceConfig")]
        public async Task<IActionResult> ProcessSourceConfig(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "harvest")] HttpRequest req,
			[Table("dashboardsourceconfig", "sourceconfig")] CloudTable sourceConfigTable,
            ILogger log)
        {
			log.LogInformation("Harvesting all sources");

			var sourceConfigEntries = await sourceConfigTable.ExecuteQuerySegmentedAsync(new TableQuery<SourceConfigTableEntity>(), null);
			var configEntriesToProcess = new List<SourceConfigTableEntity>();
			
			bool ignoreLastUpdate = false;
			if(req.Query.ContainsKey("ignoreLastUpdate"))
			{
				Boolean.TryParse(req.Query["ignoreLastUpdate"], out ignoreLastUpdate);
			}
			foreach (var configEntry in sourceConfigEntries)
			{
				// Process all sources that are enabled and are overdue.
				if(configEntry.IsEnabled && (ignoreLastUpdate || configEntry.LastUpdateUtc.AddSeconds(configEntry.IntervalSeconds) > DateTimeOffset.UtcNow))
				{
					log.LogInformation($"Config entry to be processed: {configEntry}");
					configEntriesToProcess.Add(configEntry);
				}
				else
				{
					log.LogInformation($"Config entry skipped (not enabled or not due): {configEntry}");
				}
			}

			foreach (var configEntry in configEntriesToProcess)
			{
				// For every source, run the "PersistSource" function but don't wait for it to return.
				// Functions have a an execution timeout. If there are many sources or response is slow,
				// we'd risk getting terminated by the runtime.				
				var postUrl = req.Scheme + "://" + req.Host + "/api/harvest";

				// Fire & forget. Read every source but don't wait.
				var fireAndForgetTask = postUrl
					.WithTimeout(60)
					.PostJsonAsync(configEntry)
					.ReceiveJson<SourceConfigTableEntity>();
			}
			
			return new OkObjectResult(configEntriesToProcess);
        }

		[FunctionName("PersistSource")]
        public async Task<IActionResult> PersistSource(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "harvest")] HttpRequest req,
			[Table("dashboardsourceconfig")] CloudTable sourceConfigTable,
            [Table("dashboardsourcedatahistory")] CloudTable sourceDataHistoryTable,
			ILogger log)
        {
			log.LogInformation($"Harvesting specific source");

			SourceConfigTableEntity sourceConfigEntry = null;
			var content = await new StreamReader(req.Body).ReadToEndAsync();
			try
			{
				sourceConfigEntry = JsonConvert.DeserializeObject<SourceConfigTableEntity>(content);
				log.LogInformation(sourceConfigEntry.ToString());
			}
			catch (System.Exception ex)
			{
				log.LogError($"Failed to deserialize source config data: {ex}");
				return new BadRequestObjectResult($"Failed to deserialize source config data: {ex}");
			}

			// Write back to storage with updated date/time.
			sourceConfigEntry.LastUpdateUtc = DateTimeOffset.UtcNow;
			await sourceConfigTable.ExecuteAsync(TableOperation.Replace(sourceConfigEntry));

			// Read the source.
			SourceData sourceData = null;
			try
			{
				var x = await sourceConfigEntry.Url
					.WithTimeout(180)
					.GetStringAsync();

				sourceData = await sourceConfigEntry.Url
					.WithTimeout(180)
					.GetJsonAsync<SourceData>();
			}
			catch(Exception ex)
			{
				log.LogError($"Failed to get source data for entry {sourceConfigEntry}: {ex}");
				return new BadRequestObjectResult($"Failed to get source data for entry {sourceConfigEntry}: {ex}");
			}

			// Write source result to history table.
			var sourceDataHistoryTableEntity = new SourceDataHistoryTableEntity
			{
				SourceId = sourceData.Id,
				DataItemsJson = "",
				PartitionKey = sourceData.Id,
				// Table storage does not support sorting. It sorts by partition key and
				// row key. By using an inverted date, the newest record will always be the first.
				RowKey = (DateTimeOffset.MaxValue.Ticks - sourceData.TimeStampUtc.Ticks).ToString("d19"),
				TimeStampUtc = sourceData.TimeStampUtc
			};
			var insertResult = await sourceDataHistoryTable.ExecuteAsync(TableOperation.Insert(sourceDataHistoryTableEntity));
			if(insertResult.HttpStatusCode != 200)
			{
				log.LogError($"Failed to save source history. HTTP error: {insertResult.HttpStatusCode}");
				return new BadRequestObjectResult($"Failed to save source history. HTTP error: {insertResult.HttpStatusCode}");
			}
			return new OkObjectResult(sourceConfigEntry);
        }
    }
}
