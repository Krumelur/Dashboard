using Newtonsoft.Json;

namespace Dashboard.Support.Tesla
{
	/// <summary>
	/// Charge state of one vehicle
	/// </summary>
	public class VehicleChargeState
	{
		/// <summary>
		/// Battery charge state in percent
		/// </summary>
		[JsonProperty("battery_level")]
		public int ChargeLevelPercent { get; set; }

		/// <summary>
		/// kWh charged since last plugged in
		/// </summary>
		[JsonProperty("charge_energy_added")]
		public float EnergyAdded { get; set; }

		/// <summary>
		/// Charge limit in percent
		/// </summary>
		[JsonProperty("charge_limit_soc")]
		public int ChargeLimitPercent { get; set; }

		/// <summary>
		/// Charging, idle, ...
		/// </summary>
		[JsonProperty("charging_state")]
		public string ChargingState { get; set; }

		/// <summary>
		/// Charge rate/power in kW
		/// </summary>
		[JsonProperty("charger_power")]
		public float ChargePower { get; set; }

		/// <summary>
		/// Usable kWh of battery.
		/// </summary>
		[JsonProperty("usable_battery_level")]
		public int UsableBatteryLevel { get; set; }

		public override string ToString() => $"[{nameof(VehicleChargeState)}] SOC = {ChargeLevelPercent}; State = {ChargingState}";
	}
}
