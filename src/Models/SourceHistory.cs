namespace Dashboard.Models
{
    /// <summary>
    /// Data structure returned by when requesting a source's history.
    /// </summary>
    public class SourceHistory
	{
		public SourceConfig SourceConfig { get; set; }
		public SourceData[] HistoryData { get; set; }
	}
}
