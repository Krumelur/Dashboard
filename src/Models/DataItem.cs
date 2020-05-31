using System;
using System.Collections.Generic;

namespace Dashboard.Models
{
	public enum DataItemType
	{
		Text,
		Integer,
		Decimal,
		Percentage,
		Bar,
	}

    /// <summary>
    /// Individual piece of data returned by a source.
    /// </summary>
    public class DataItem
	{
		public string Id { get; set; }
		public DataItemType Type { get; set; }
		public string Label { get; set; }

		public object Value { get; set; }

		public object ReferenceValue { get; set; }

		public override string ToString() => $"[{nameof(DataItem)}] Id = '{Id}', Type = '{Type}', Label = '{Label}', Value = '{Value}', Reference value = '{ReferenceValue}'";
	}
}
