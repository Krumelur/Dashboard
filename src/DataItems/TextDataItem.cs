namespace DataItems
{
	/// <summary>
	/// A data item containing text data.
	/// </summary>
	public class TextDataItem : DataItem
	{
		/// <summary>
		/// Label to show on the dashboard.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Value of the item to show on the dashboard.
		/// </summary>
		public string Value { get; set; }
	}
}
