using System;
using System.Collections.Generic;

namespace DataItems
{
	/// <summary>
	/// Data structure returned by a source.
	/// </summary>
	public class SourceData
	{
		/// <summary>
		/// Identifies this source. Identifiers are for example "WOD", "Solar", "EV".
		/// </summary>
		public string Id { get; set; }
			
		/// <summary>
		/// Defines when this source data was created.
		/// </summary>
		public DateTimeOffset TimeStampUtc { get; set; }

		/// <summary>
		/// The title of the source to be displayed on the dashboard.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The individual data items the source provides. The "Solar" source may for example return items for daily production, current power and peak power.
		/// </summary>
		public List<DataItem> DataItems { get; set; }

		public override string ToString() => $"[{nameof(SourceData)}] Id = '{Id}', Title = '{Title}', TimeStamp = '{TimeStampUtc}'";
	}
}
