using Newtonsoft.Json;

namespace TeslaApi.Responses
{
	public class VehicleDataResponse
	{
		/// <summary>
		/// ID of the vehicle
		/// </summary>
		[JsonProperty("id")]
		public long Id { get; set; }

		/// <summary>
		/// User ID (?)
		/// </summary>
		[JsonProperty("user_id")]
		public long UserId { get; set; }

		/// <summary>
		/// Vehicle ID
		/// </summary>
		[JsonProperty("vehicle_id")]
		public long VehicleId { get; set; }

		/// <summary>
		/// VIN
		/// </summary>
		[JsonProperty("vin")]
		public string VIN { get; set; }

		/// <summary>
		/// Name of the vehicle
		/// </summary>
		[JsonProperty("display_name")]
		public string Name { get; set; }

		/// <summary>
		/// Color of the vehicle
		/// </summary>
		[JsonProperty("color")]
		public string Color { get; set; }

		/// <summary>
		/// State of the vehicle
		/// </summary>
		[JsonProperty("state")]
		public string Status { get; set; }

		/// <summary>
		/// Flag if car is in service mode
		/// </summary>
		[JsonProperty("in_service")]
		public string IsInService { get; set; }

		/// <summary>
		/// Information about SoC
		/// </summary>
		[JsonProperty("charge_state")]
		public VehicleChargeState ChargeState { get; set; }

		/// <summary>
		/// Information about climate
		/// </summary>
		[JsonProperty("climate_state")]
		public VehicleClimateState ClimateState { get; set; }

		/// <summary>
		/// Information about drive state
		/// 
		/// </summary>
		[JsonProperty("drive_state")]
		public DriveStateResponse DriveState { get; set; }

	}
}

