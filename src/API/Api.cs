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
using Newtonsoft.Json;

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
			[CosmosDB("dashboard", "sourcedata", ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient docClient,
			string sourceId,
			ILogger log)
		{
			log.LogInformation($"Getting source data for '{sourceId}'");
			
			var configItemsToProcess = new List<SourceConfig>();

			if(int.TryParse(req.Query["numdatapoints"], out int numDataPoints))
			{
				log.LogInformation($"Number of data points was specified: '{1}'");
			}
			else
			{
				numDataPoints = 1;
			}

			var sensitiveDataPin = _config["SensitiveDataPin"];
			bool isSensitiveDataPinPresent = Helpers.IsSensitiveDataPinProvided(sensitiveDataPin, req);

			var collectionUri = UriFactory.CreateDocumentCollectionUri("dashboard", "sourcedata");

			// SELECT TOP 2 * FROM c WHERE c.SourceId='solar' ORDER BY c.TimeStampUtc DESC
			var query = docClient
				.CreateDocumentQuery<SourceData>(collectionUri)
				.Where(x => x.SourceId == sourceId)
				.OrderByDescending(x => x.TimeStampUtc)
				.Take(numDataPoints)
				.AsDocumentQuery();

			var dataHistoryItems = new List<SourceData>();

			while(query.HasMoreResults)
			{
				var documents = await query.ExecuteNextAsync<SourceData>();

				// Filter all data items in the document and exclude sensitive ones if the PIN is not provided.
				foreach (var document in documents)
				{
					document.DataItems = document
						.DataItems
						.Where(x => !x.IsSensitive || isSensitiveDataPinPresent)
						.ToArray();
				}
				dataHistoryItems.AddRange(documents);
			}

			var result = new SourceHistory {
				SourceId = sourceId,
				HistoryData = dataHistoryItems.ToArray()
			};
			
			return new OkObjectResult(result);
		}

		/// <summary>
		/// HTTP triggered function (POST).
		/// 
		/// Query parameters:
		/// numdatapoints		Number of data points to retrieve. Defaults to 1 (the latest).
		/// </summary>
		/// <returns></returns>
		[FunctionName("GetSourceDataFiltered")]
		public async Task<IActionResult> GetSourceDataFiltered(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = "sourcedata")] HttpRequest req,
			[CosmosDB("dashboard", "sourcedata", ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient docClient,
			ILogger log)
		{
			log.LogInformation("Getting filtered source data.");
			
			Filter filter = null;
			try
			{
				filter = JsonConvert.DeserializeObject<Filter>(await req.ReadAsStringAsync());	
			}
			catch (System.Exception ex)
			{
				return new BadRequestObjectResult("Error deserializing Filter from request body: " + ex);
			}			

			var configItemsToProcess = new List<SourceConfig>();

			var sensitiveDataPin = _config["SensitiveDataPin"];
			bool isSensitiveDataPinPresent = Helpers.IsSensitiveDataPinProvided(sensitiveDataPin, req);

			var collectionUri = UriFactory.CreateDocumentCollectionUri("dashboard", "sourcedata");

			IQueryable<SourceData> query = docClient.CreateDocumentQuery<SourceData>(collectionUri);

			// Filter by source ID.
			string sourceId = null;
			if(!string.IsNullOrWhiteSpace(filter.SourceId))
			{
				sourceId = filter.SourceId.ToLowerInvariant().Trim();
				query = query.Where(x => x.SourceId == sourceId);
			}

			// Filter by start date.
			if(filter.StartDateUtc.HasValue)
			{
				query = query.Where(x => x.TimeStampUtc >= filter.StartDateUtc);
			}

			// Filter by end date.
			if(filter.EndDateUtc.HasValue)
			{
				query = query.Where(x => x.TimeStampUtc < filter.EndDateUtc);
			}
				
			var filterQuery = query
				.OrderByDescending(x => x.TimeStampUtc)
				.Take(filter.TakeNumberResults.HasValue ? filter.TakeNumberResults.Value : int.MaxValue)
				.AsDocumentQuery();

			var dataHistoryItems = new List<SourceData>();

			while(filterQuery.HasMoreResults)
			{
				var documents = await filterQuery.ExecuteNextAsync<SourceData>();

				// Filter all data items in the document and exclude sensitive ones if the PIN is not provided.
				foreach (var document in documents)
				{
					document.DataItems = document
						.DataItems
						.Where(x => !x.IsSensitive || isSensitiveDataPinPresent)
						.ToArray();
				}
				dataHistoryItems.AddRange(documents);
			}

			var result = new SourceHistory {
				SourceId = sourceId,
				HistoryData = dataHistoryItems.ToArray()
			};
			
			return new OkObjectResult(result);
		}
	}
}
