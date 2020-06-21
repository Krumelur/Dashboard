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
using System.Net.Http;

namespace Dashboard.Server.Harvester
{
    public class Api
    {
		public Api(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

		/// <summary>
		/// HTTP triggered function (GET).
		/// </summary>
		/// <returns></returns>
        [FunctionName("GetSourceDataHistory")]
        public async Task<IActionResult> GetSourceDataHistory(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "sourcedata")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<SourceConfigItem> sourceConfigItems,
            ILogger log)
        {
			log.LogInformation("Getting persisted source data");

			var configItemsToProcess = new List<SourceConfigItem>();
			
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

			var harvesterFuncAuthKey = _config["HarvesterFunctionsAuthKey"];
			if(String.IsNullOrWhiteSpace(harvesterFuncAuthKey))
			{
				return new BadRequestObjectResult("Failed to retrieve authorization key. Is it added to the configuration?");
			}

			foreach (var sourceConfigItem in sourceConfigItems)
			{
				// Process all sources that are enabled and are overdue.
				if(sourceConfigItem.IsEnabled && (ignoreLastUpdate || sourceConfigItem.LastUpdateUtc == DateTimeOffset.MinValue || sourceConfigItem.LastUpdateUtc.AddMinutes(sourceConfigItem.IntervalMinutes) < DateTimeOffset.UtcNow))
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
				var postUrl = req.Scheme + "://" + req.Host + "/api/harvestsource";

				log.LogInformation($"About to harvest source ID '{sourceConfigItem.Id}' ('{sourceConfigItem.Name}') using URL '{postUrl}'");
				
				try
				{
					// Fire & forget. Read every source but don't wait.
					var fireAndForgetTask = postUrl
						.WithHeader("x-functions-key", harvesterFuncAuthKey)
						.WithTimeout(60)
						.PostJsonAsync(sourceConfigItem)
						.ReceiveJson<SourceConfigItem>();

					if(awaitCompletion)
					{
						log.LogInformation($"Awaiting completion of source ID '{sourceConfigItem.Id}' ('{sourceConfigItem.Name}')");
						var harvestedData = await fireAndForgetTask;

						log.LogInformation($"Received source config item: '{harvestedData}'");
					}
				}
				catch(Exception ex)
				{
					log.LogError($"Error while calling harvester for source ID '{sourceConfigItem.Id}' ('{sourceConfigItem.Name}') using URL '{postUrl}': {ex}");
				}
			}
			
			return new OkObjectResult(configItemsToProcess);
        }
    }	
}
