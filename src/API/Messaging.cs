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
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Dashboard.API
{
	public class Messaging
	{
		public Messaging(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

		[FunctionName("HistoryEntryAdded")]
		public async Task HistoryEntryAdded([CosmosDBTrigger(databaseName: "dashboard", collectionName: "sourcedata", ConnectionStringSetting = "CosmosDbConnectionString", LeaseCollectionName = "leases")] IReadOnlyList<Document> input,
		[SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> signalRMessages,
		ILogger log)
		{
			if (input == null || input.Count <= 0)
			{
				return;
			}

			log.LogInformation("Documents modified " + input.Count);
			log.LogInformation("First document Id " + input[0].Id);

			await signalRMessages.AddAsync(
				new SignalRMessage
				{
					Target = "SourceUpdated",
					Arguments = new[] { "pellets" }
				});
		}
	}
}
