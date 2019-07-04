namespace ETASupport
{
	public class PelletsData
	{
		public int SuppliesKg { get; set; }

		public override string ToString() => $"[{nameof(PelletsData)} Supplies = {SuppliesKg}kg]";
	}
}
