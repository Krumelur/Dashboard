using System;

namespace Dashboard.Models
{
	public class Filter
	{
		public string SourceId { get; set; }
		public DateTime? StartDateUtc { get; set; }
		public DateTime? EndDateUtc { get; set; }
		public int? TakeNumberResults { get; set; }
		
	}
}