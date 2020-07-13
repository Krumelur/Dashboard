using System;
using Newtonsoft.Json;

namespace Dashboard.Models
{
    /// <summary>
    /// Data structure returned by a source.
    /// </summary>
    public class SourceData
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
		[JsonProperty("sourceid")]
		public string SourceId { get; set; }
			
		/// <summary>
		/// Defines when this source data was created.
		/// </summary>
		public DateTimeOffset TimeStampUtc { get; set; }

		/// <summary>
		/// The individual data items the source provides. The "Solar" source may for example return items for daily production, current power and peak power.
		/// </summary>
		public DataItem[] DataItems { get; set; }

		public override string ToString() => $"[{nameof(SourceData)}] Id = '{Id}', TimeStamp = '{TimeStampUtc}'";
	}
}
