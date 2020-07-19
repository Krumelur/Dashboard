using Dashboard.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using System.Collections;
using System;
using Cronos;

namespace Dashboard.API
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
		/// 
		/// Query parameters:
		/// numdatapoints		Number of data points to retrieve. Defaults to 1 (the latest).
		/// </summary>
		/// <returns></returns>
		[FunctionName("GetSourceData")]
		public async Task<IActionResult> GetSourceData(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = "sourcedata/{sourceId}")] HttpRequest req,
			[CosmosDB("dashboard", "sourceconfig", ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<SourceConfig> SourceConfigs,
			[CosmosDB("dashboard", "sourcedata", ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient docClient,
			string sourceId,
			ILogger log)
		{
			log.LogInformation($"Getting source data for '{sourceId}'");
			
			var configItemsToProcess = new List<SourceConfig>();

			var authKey = _config["StandardPermsFunctionsAuthKey"];
			if(!Helpers.CheckFunctionAuthKey(authKey, req))
			{
				return new BadRequestObjectResult("Failed to verify StandardPermsFunctionsAuthKey.");
			}

			if(int.TryParse(req.Query["numdatapoints"], out int numDataPoints))
			{
				log.LogInformation($"Number of data points was specified: '{1}'");
			}
			else
			{
				numDataPoints = 1;
			}

			var sourceConfig = SourceConfigs.FirstOrDefault(x => x.Id == sourceId);
			if(sourceConfig == null)
			{
				log.LogWarning($"Unable to find source config item for ID '{sourceId}'");
			}

			var collectionUri = UriFactory.CreateDocumentCollectionUri("dashboard", "sourcedata");

			// SELECT TOP 2 * FROM c WHERE c.SourceId='solar' ORDER BY c.TimeStampUtc DESC
			var query = docClient.CreateDocumentQuery<SourceData>(collectionUri)
			.Where(x => x.SourceId == sourceId)
			.OrderByDescending(x => x.TimeStampUtc)
			.Take(numDataPoints)
			.AsDocumentQuery();

			var dataHistoryItems = new List<SourceData>();

			while(query.HasMoreResults)
			{
				var documents = await query.ExecuteNextAsync<SourceData>();
				dataHistoryItems.AddRange(documents);
			}

			var result = new SourceHistory {
				SourceConfig = sourceConfig,
				HistoryData = dataHistoryItems.ToArray()
			};
			
			return new OkObjectResult(result);
		}
	}
}
