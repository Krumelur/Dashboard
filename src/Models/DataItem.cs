namespace Dashboard.Models
{
	public enum DataItemType
	{
		Text = 1,
		Integer = 1,
		Decimal = 2,
	}

    /// <summary>
    /// Individual piece of data returned by a source.
    /// </summary>
    public class DataItem
	{
		public string Id { get; set; }

		public DataItemType Type { get; set; } = DataItemType.Text;

		public string Label { get; set; }

		public object Value { get; set; }

		public object ReferenceValue { get; set; }

		public override string ToString() => $"[{nameof(DataItem)}] Id = '{Id}', Type = '{Type}', Label = '{Label}', Value = '{Value}', Reference value = '{ReferenceValue}'";
	}
}
