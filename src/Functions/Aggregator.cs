using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DTOs;
using Flurl.Http;
using Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Functions
{
	public static class Aggregator
	{
		[FunctionName("Aggregator")]
		public static async Task RunOrchestrator(
			[OrchestrationTrigger] DurableOrchestrationContext context,
			[Table("history")] CloudTable historyTable,
			ILogger log)
		{
			var registryEntries = context.GetInput<List<RegistryEntry>>();
			log.LogInformation($"Running Aggregator...processing {registryEntries.Count} entries...");

			var tasks = new List<Task<SourceData>>();
			foreach (var item in registryEntries)
			{
				var task = context.CallActivityAsync<SourceData>("Aggregator_ReadSource", item);
				tasks.Add(task);
			}

			while (tasks.Count > 0)
			{
				log.LogInformation($"Tasks left: {tasks.Count}...");
				var completedTask = await Task.WhenAny(tasks);

				var sourceDto = completedTask.Result;
				var sourceEntity = sourceDto.ToTableEntity();
				if (sourceEntity != null)
				{
					var op = TableOperation.InsertOrReplace(sourceEntity);
					var tableResult = await historyTable.ExecuteAsync(op);

					if (tableResult.HttpStatusCode < 200 || tableResult.HttpStatusCode > 300)
					{
						log.LogError($"Failed to store source item in storage. HTTP status code: {tableResult.HttpStatusCode}, Item: {sourceDto}");
					}
				}
				else
				{
					log.LogError($"Failed to convert source data '{sourceDto}' to entity.");
				}
				tasks.Remove(completedTask);
			}
		}

		[FunctionName("Aggregator_ReadSource")]
		public async static Task<SourceData> ReadSource([ActivityTrigger] RegistryEntry registryEntry, ILogger log)
		{
			log.LogInformation($"Reading source data for: {registryEntry}");

			var url = registryEntry.Url;
			if (string.IsNullOrWhiteSpace(url))
			{
				log.LogError($"Registry entry has no valid url: {registryEntry}");
				return null;
			}

			SourceData sourceData = null;
			try
			{
				sourceData = await url
					.WithTimeout(60)
					.GetJsonAsync<SourceData>();
			}
			catch (Exception ex)
			{
				log.LogError($"Failed to get source data for entry {registryEntry}: {ex}");
				return null;
			}
			return sourceData;
		}

		static async Task<string> ProcessSourceRegistry(DurableOrchestrationClient starter, CloudTable registryTable, ILogger log)
		{
			log.LogInformation($"Reading registry entries...");
			var registryEntries = await registryTable.ExecuteQuerySegmentedAsync(new TableQuery<RegistryEntry>(), null);
			log.LogInformation($"Done reading registry entries and found:");
			foreach (var item in registryEntries)
			{
				log.LogInformation(item.ToString());
			}

			string instanceId = await starter.StartNewAsync("Aggregator", registryEntries.Where(r => r.IsEnabled));
			log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

			return instanceId;
		}

		/// <summary>
		/// Kicks off processing the source registry by HTTP GET.
		/// </summary>
		/// <param name="req"></param>
		/// <param name="starter"></param>
		/// <param name="registryTable">the registry table in Azure Storage Tables to read registry data from</param>
		/// <param name="log"></param>
		/// <returns></returns>
		[FunctionName("AggregatorStartHttp")]
		public static async Task<HttpResponseMessage> AggregatorStartHttp(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
			[OrchestrationClient]DurableOrchestrationClient starter,
			[Table("sourceregistry", "RegistryEntry")] CloudTable registryTable,
			ILogger log)
		{
			log.LogInformation($"Aggregator started by HTTP GET.");
			log.LogInformation($"Active instances around? {(await HasActiveInstances(starter) ? "YES" : "NO")}");

			var instanceId = await ProcessSourceRegistry(starter, registryTable, log);
			return starter.CreateCheckStatusResponse(req, instanceId);
		}

		/// <summary>
		/// Triggers the aggregator based on a timer. The timer (CRON expression) is read from an app setting called "ScheduleAggregatorCRON"
		/// </summary>
		/// <param name="timerInfo"></param>
		/// <param name="starter"></param>
		/// <param name="registryTable">the registry table in Azure Storage Tables to read registry data from</param>
		/// <param name="log"></param>
		/// <returns></returns>
		[FunctionName("AggregatorStartTimer")]
		public static async Task AggregatorStartTimer(
#if DEBUG
			[TimerTrigger("%ScheduleAggregatorCRON%", RunOnStartup = true)] TimerInfo timerInfo,
#else
			[TimerTrigger("%ScheduleAggregatorCRON%", RunOnStartup = false)] TimerInfo timerInfo,
#endif
			[OrchestrationClient]DurableOrchestrationClient starter,
			[Table("sourceregistry", "RegistryEntry")] CloudTable registryTable,
			ILogger log)
		{
			log.LogInformation($"Aggregator started by timer. Next occurence will by at {timerInfo.Schedule.GetNextOccurrence(DateTime.Now)}");

			if (await HasActiveInstances(starter))
			{
				log.LogWarning("Aggregator was triggered while previous instance was still active. Skipping.");
				return;
			}
			
			await ProcessSourceRegistry(starter, registryTable, log);
		}

		/// <summary>
		/// Checks if there is at least one instance of the orchestrator that is still active.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		static async Task<bool> HasActiveInstances(DurableOrchestrationClient client)
		{
			var instances = await client.GetStatusAsync();
			foreach (var instance in instances)
			{
				if (instance.RuntimeStatus != OrchestrationRuntimeStatus.Canceled
					&& instance.RuntimeStatus != OrchestrationRuntimeStatus.Completed
					&& instance.RuntimeStatus != OrchestrationRuntimeStatus.Failed
					&& instance.RuntimeStatus != OrchestrationRuntimeStatus.Terminated)
				{
					return true;
				}
			}

			return false;
		}
	}
}