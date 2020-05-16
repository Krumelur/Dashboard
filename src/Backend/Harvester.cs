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
	public class SourceConfigEntry : TableEntity
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public DateTimeOffset LastUpdateUtc { get; set; }
		public int IntervalSeconds { get; set; }
		public bool IsEnabled { get; set; }

		public override string ToString() => $"[{nameof(SourceConfigEntry)}] Name = '{Name}', Url= '{Url}', LastUpdateUtc = '{LastUpdateUtc}', IntervalSeconds = '{IntervalSeconds}', IsEnabled = '{IsEnabled}'";
	}

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

			var sourceConfigEntries = await sourceConfigTable.ExecuteQuerySegmentedAsync(new TableQuery<SourceConfigEntry>(), null);
			var enabledConfigEntries = new List<SourceConfigEntry>();
			foreach (var item in sourceConfigEntries)
			{
				// TODO: Add check if update is required (LastUpdateUtc)
				if(item.IsEnabled)
				{
					enabledConfigEntries.Add(item);
				}
				log.LogInformation(item.ToString());
			}

			var allSourceData = new List<dynamic>();
			foreach (var configEntry in enabledConfigEntries)
			{
				// For every source, run the "ReadSource" function but don't wait for it to return.
				// Functions have a an execution timeout. If there are many sources or response is slow,
				// we'd risk getting terminated by the runtime.				
				var postUrl = req.Scheme + "://" + req.Host + "/api/harvest";

				// Fire & forget. Read every source but don't wait.
				/*var ret = await*/ postUrl
					.WithTimeout(60)
					.PostJsonAsync(configEntry)
					.ReceiveJson<SourceConfigEntry>();
			}
			
			return new OkObjectResult(enabledConfigEntries);
        }

		[FunctionName("PersistSource")]
        public async Task<IActionResult> PersistSource(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "harvest")] HttpRequest req,
			[Table("dashboardsourceconfig")] CloudTable sourceConfigTable,
            [Table("dashboardsourcedata")] CloudTable sourceDataTable,
			ILogger log)
        {
			log.LogInformation($"Harvesting specific source");

			SourceConfigEntry sourceConfigEntry = null;
			var content = await new StreamReader(req.Body).ReadToEndAsync();
			try
			{
				sourceConfigEntry = JsonConvert.DeserializeObject<SourceConfigEntry>(content);
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
			var sourceJson = await sourceConfigEntry.Url
				.WithTimeout(60)
				.GetJsonAsync();
			
			// TODO: Cannot insert dynamic
			await sourceDataTable.ExecuteAsync(TableOperation.Insert(sourceJson));

			return new OkObjectResult(sourceConfigEntry);
        }
    }
}
