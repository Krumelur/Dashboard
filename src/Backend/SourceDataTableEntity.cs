using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Functions
{
    public class SourceDataHistoryTableEntity : TableEntity
	{
		/// <summary>
		/// Identifies this source. Identifiers are for example "WOD", "Solar", "EV".
		/// </summary>
		public string SourceId { get; set; }

		/// <summary>
		/// Defines when this history entry was created.
		/// </summary>
		public DateTimeOffset TimeStampUtc { get; set; }

		/// <summary>
		/// The individual data items. The "Solar" source may for example return items for daily production, current power and peak power.
		/// </summary>
		public string DataItemsJson { get; set; }

		public override string ToString() => $"[{nameof(SourceConfigTableEntity)}] SourceId = '{SourceId}', TimeStampUtc = '{TimeStampUtc}'";
	}
}
