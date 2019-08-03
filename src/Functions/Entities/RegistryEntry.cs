using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Functions.Entities
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
}
