using DTOs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace Functions.Entities
{
	/// <summary>
	/// Composition object to wrap a <see cref="SourceData"/> item into a <see cref="TableEntity"/> so it can be stored in a Table Storage.
	/// </summary>
	public class SourceDataEntity : TableEntity
	{
		public SourceDataEntity(SourceData sourceData)
		{
			SourceData = sourceData;
		}

		/// <summary>
		/// The source data item.
		/// </summary>
		[EntityJsonPropertyConverter]
		public SourceData SourceData { get; }

		public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
		{
			var results = base.WriteEntity(operationContext);
			EntityJsonPropertyConverter.Serialize(this, results);
			return results;
		}

		public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
		{
			base.ReadEntity(properties, operationContext);
			EntityJsonPropertyConverter.Deserialize(this, properties);
		}
	}
}
