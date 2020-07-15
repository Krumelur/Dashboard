using Newtonsoft.Json;

namespace Dashboard.Server.DataSources.Tesla.Responses
{
	public class VehicleStateResponse
	{
		[JsonProperty("api_version")]
		public int ApiVersion { get; set; }

		[JsonProperty("car_version")]
		public string SoftwareVersion { get; set; }

		[JsonProperty("is_user_present")]
		public bool IsUserPresent { get; set; }

		[JsonProperty("locked")]
		public bool IsLocked { get; set; }

		[JsonProperty("odometer")]
		public double Odometer { get; set; }

		[JsonProperty("remote_start")]
		public bool IsRemoteStarted { get; set; }

		[JsonProperty("remote_start_supported")]
		public bool IsRemoteStartSupported { get; set; }

		[JsonProperty("sun_roof_percent_open")]
		public int? SunRoofPercentOpen { get; set; }

		[JsonProperty("sun_roof_state")]
		public string SunRoofState { get; set; }

		[JsonProperty("timestamp")]
		public long Timestamp { get; set; }

		[JsonProperty("valet_mode")]
		public bool IsValetMode { get; set; }

		[JsonProperty("valet_pin_needed")]
		public bool IsPinProtected { get; set; }

		[JsonProperty("vehicle_name")]
		public string VehicleName { get; set; }
	}
}

