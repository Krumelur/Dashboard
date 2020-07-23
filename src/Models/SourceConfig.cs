using System;
using Newtonsoft.Json;

namespace Dashboard.Models
{
    public class SourceConfig
	{
		/// <summary>
		/// ID of the config item.
		/// CosmosDB expects the property to be lowercase.
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }
			
		/// <summary>
		/// Name of the configuration item, for example "Solar".
		/// This property is used as the partition key in CosmosDB and must be lowercase.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
		
		public DateTimeOffset? LastUpdateUtc { get; set; }

		public string CronExecutionTime { get; set; }

		public DateTimeOffset? NextExecutionDueUtc { get; set; }
		
		public bool IsEnabled { get; set; }

		public override string ToString() => $"[{nameof(SourceConfig)}] Name = '{Name}', LastUpdateUtc = '{LastUpdateUtc}', CronExecutionTime = '{CronExecutionTime}', IsEnabled = '{IsEnabled}'";
	}
}