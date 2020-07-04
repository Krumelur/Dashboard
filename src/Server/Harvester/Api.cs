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
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using System.Collections;

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
		[FunctionName("GetLatestSourceDataHistory")]
		public async Task<IActionResult> GetLatestSourceDataHistory(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "latestsourcedatahistory")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<SourceConfigItem> sourceConfigItems,
			[CosmosDB("dashboard", "sourcedatahistory", ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient docClient,
			ILogger log)
		{
			log.LogInformation("Getting latest source data history");

			var configItemsToProcess = new List<SourceConfigItem>();

			var harvesterFuncAuthKey = _config["HarvesterFunctionsAuthKey"];
			if (String.IsNullOrWhiteSpace(harvesterFuncAuthKey))
			{
				return new BadRequestObjectResult("Failed to retrieve authorization key. Is it added to the configuration?");
			}

			foreach (var sourceConfigItem in sourceConfigItems)
			{
				// Process all sources that are enabled and are overdue.
				if (sourceConfigItem.IsEnabled)
				{
					log.LogInformation($"Config entry to be processed: {sourceConfigItem}");
					configItemsToProcess.Add(sourceConfigItem);
				}
				else
				{
					log.LogInformation($"Config entry skipped (not enabled): {sourceConfigItem}");
				}
			}

			var latestHistoryItems = new ArrayList();
			
			// Iterate over all enabled sources and get the latest history entry.
			var collectionUri = UriFactory.CreateDocumentCollectionUri("dashboard", "sourcedatahistory");

			foreach (var sourceConfigItem in configItemsToProcess)
			{
				var query = docClient.CreateDocumentQuery<SourceDataHistoryItem>(collectionUri)
				.Where(x => x.Id == sourceConfigItem.LatestHistoryItemId)
				.AsDocumentQuery();

				var latestSourceDataHistoryItem = (await query.ExecuteNextAsync()).FirstOrDefault();

				latestHistoryItems.Add(new {
					SourceName = sourceConfigItem.Name,
					LatestHistoryItem = latestSourceDataHistoryItem
				});
			}

			return new OkObjectResult(latestHistoryItems);
		}
	}
}
