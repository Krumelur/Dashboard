namespace DataItems
{
	/// <summary>
	/// Base class for all data items in a <see cref="SourceData"/> element.
	/// </summary>
	public abstract class DataItem
	{
		/// <summary>
		/// Identifier of the item. For a "Solar" source data item this could be "PEAKPOWER" for the item containing the peak power of the solar installation.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Returns the specific item type name that derives from this abstract base class, for example "TextDataItem" for an item containing text data.
		/// </summary>
		public string ItemTypeName => GetType().Name;
	}
}