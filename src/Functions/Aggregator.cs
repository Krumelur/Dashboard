using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Functions
{
	public class RegistryEntry : TableEntity
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public DateTimeOffset LastUpdateUtc { get; set; }
		public int IntervalSeconds { get; set; }
		public bool IsEnabled { get; set; }

		public override string ToString() => $"[{nameof(RegistryEntry)}] Name = '{Name}', Url= '{Url}', LastUpdateUtc = '{LastUpdateUtc}', IntervalSeconds = '{IntervalSeconds}', IsEnabled = '{IsEnabled}'";
	}

	public static class Aggregator
    {
        [FunctionName("Aggregator")]
		public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
			[Table("history")] CloudTable historyTable,
			ILogger log)
		{
			var registryEntries = context.GetInput<List<RegistryEntry>>();
			log.LogInformation($"Running Aggregator...processing {registryEntries.Count} entries...");

			var tasks = new List<Task<SourceDTO>>();
			foreach (var item in registryEntries)
			{
				var task = context.CallActivityAsync<SourceDTO>("Aggregator_ReadSource", item);
				tasks.Add(task);
			}

			while (tasks.Count > 0)
			{
				log.LogInformation($"Tasks left: {tasks.Count}...");
				var completedTask = await Task.WhenAny(tasks);
				var sourceDto = completedTask.Result;
				await historyTable.CreateAsync(sourceDto);
				tasks.Remove(completedTask);
			}

			

			return "OK";
        }

        [FunctionName("Aggregator_ReadSource")]
        public async static Task<SourceDTO> ReadSource([ActivityTrigger] RegistryEntry registryEntry, ILogger log)
        {
            log.LogInformation($"Reading source data for: {registryEntry}");

			var rand = new Random();
			var delay = rand.Next(2000, 5000);
			log.LogInformation($"Sleeping for {delay}ms...");
			await Task.Delay(delay);

			return new SourceDTO {
				Id = registryEntry.Name
			};

        }

        [FunctionName("AggregatorStart")]
        public static async Task<HttpResponseMessage> Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
			[Table("sourceregistry", "RegistryEntry")] CloudTable registryTable,
			ILogger log)
        {
			var registryEntries = await registryTable.ExecuteQuerySegmentedAsync(new TableQuery<RegistryEntry>(), null);
			log.LogInformation($"Read registry entries and found:");
			foreach (var item in registryEntries)
			{
				log.LogInformation(item.ToString());
			}

			string instanceId = await starter.StartNewAsync("Aggregator", registryEntries.Where(r => r.IsEnabled));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}