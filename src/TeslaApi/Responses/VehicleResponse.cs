using Newtonsoft.Json;

namespace TeslaApi.Responses
{
	/// <summary>
	/// Data of one vehicle
	/// </summary>
	public class VehicleResponse
	{
		/// <summary>
		/// ID of the vehicle
		/// </summary>
		[JsonProperty("id")]
		public long Id { get; set; }

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
		/// Status
		/// </summary>
		[JsonProperty("state")]
		public string Status { get; set; }

		/// <summary>
		/// Flag if car is in service mode
		/// </summary>
		[JsonProperty("in_service")]
		public string IsInService { get; set; }

		public override string ToString() => $"[{nameof(VehicleResponse)}] Id = {Id}; Name = {Name}; VIN = {VIN}";
	}
}
