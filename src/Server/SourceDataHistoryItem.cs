using System;
using Newtonsoft.Json;

namespace Dashboard.Server
{
    public class SourceDataItem
	{
		/// <summary>
		/// ID of the history item.
		/// CosmosDB expects the property to be lowercase.
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		/// Identifies this source. Identifiers are for example "WOD", "Solar", "EV".
		/// This property is used as the partition key in CosmosDB.
		/// </summary>
		public string SourceId { get; set; }

		/// <summary>
		/// Defines when this history entry was created.
		/// </summary>
		public DateTimeOffset TimeStampUtc { get; set; }

		/// <summary>
		/// The individual data items. The "Solar" source may for example return items for daily production, current power and peak power.
		/// </summary>
		public object[] DataItems { get; set; }

		public override string ToString() => $"[{nameof(SourceConfigItem)}] SourceId = '{SourceId}', TimeStampUtc = '{TimeStampUtc}'";
	}
}
