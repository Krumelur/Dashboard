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
				if(item.IsEnabled)
				{
					enabledConfigEntries.Add(item);
				}
				log.LogInformation(item.ToString());
			}

			var allSourceData = new List<dynamic>();
			foreach (var configEntry in enabledConfigEntries)
			{
				var postUrl = req.Scheme + "://" + req.Host + "/api/harvest";

				var ret = await postUrl
					.WithTimeout(60)
					.PostJsonAsync(configEntry)
					.ReceiveJson<SourceConfigEntry>();
			}
			
			return new OkObjectResult(enabledConfigEntries);
        }

		[FunctionName("ReadSource")]
        public async Task<IActionResult> ReadSource(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "harvest")] HttpRequest req,
            ILogger log)
        {
			log.LogInformation($"Harvesting specific source");

			SourceConfigEntry sourceConfigEntry = null;
			var content = await new StreamReader(req.Body).ReadToEndAsync();
			try
			{
				sourceConfigEntry = JsonConvert.DeserializeObject<SourceConfigEntry>(content);
			}
			catch (System.Exception ex)
			{
				log.LogError($"Failed to deserialize source config data: {ex}");
				return new BadRequestObjectResult($"Failed to deserialize source config data: {ex}");
			}

			dynamic sourceData = null;
			try
			{
				sourceData = await sourceConfigEntry.Url
					.WithTimeout(60)
					.GetJsonAsync();
			}
			catch (Exception ex)
			{
				log.LogError($"Failed to get source data for entry {sourceConfigEntry}: {ex}");
				return new BadRequestObjectResult($"Failed to get source data for entry {sourceConfigEntry}: {ex}");
			}

			return new OkObjectResult(sourceData);
        }
    }
}
