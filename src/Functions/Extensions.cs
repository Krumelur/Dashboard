using DTOs;
using Functions.Entities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Functions
{
	public static class Extensions
	{
		/// <summary>
		/// Converts a <see cref="SourceData"/> item into a <see cref="TableEntity"/> so it can be stored.
		/// </summary>
		/// <param name="sourceData"></param>
		/// <returns></returns>
		public static ITableEntity ToTableEntity(this SourceData sourceData) => sourceData == null ? null : new SourceDataEntity(sourceData)
		{
			PartitionKey = sourceData.Id,
			// s = 2008-10-01T17:04:32
			RowKey = sourceData.TimeStampUtc.ToString("s")
		};
	}
}
