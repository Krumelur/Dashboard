using System;
using System.Collections.Generic;

namespace DTOs
{
	public class SourceDTO
	{
		public string Id { get; set; }
		public DateTimeOffset TimeStampUtc { get; set; }
		public string Title { get; set; }
		public List<BaseDataDTO> DataItems { get; set; }

		public override string ToString() => $"[{nameof(SourceDTO)}] Id = '{Id}', Title = '{Title}', TimeStamp = '{TimeStampUtc}'";
	}
}
