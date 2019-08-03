using Newtonsoft.Json;

namespace TeslaApi.Responses
{
	/// <summary>
	/// Climate state of one vehicle
	/// </summary>
	public class VehicleClimateState
	{
		/// <summary>
		/// Status of battery heater
		/// </summary>
		[JsonProperty("battery_heater")]
		public bool BatteryHeaterEnabled { get; set; }

		/// <summary>
		/// Inside temperature of vehicle
		/// </summary>
		[JsonProperty("inside_temp")]
		public float InsideTemperature { get; set; }
		
		/// <summary>
		/// Status of climate control
		/// </summary>
		[JsonProperty("is_climate_on")]
		public bool ClimateEnabled { get; set; }
		
		public override string ToString() => $"[{nameof(VehicleClimateState)}] Inside temperature = {InsideTemperature}; Climate on = {ClimateEnabled}";
	}
}
