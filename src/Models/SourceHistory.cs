namespace Dashboard.Models
{
    /// <summary>
    /// Data structure returned by when requesting a source's history.
    /// </summary>
    public class SourceHistory
	{
		public string SourceId { get; set; }
		public SourceData[] HistoryData { get; set; }
	}
}
