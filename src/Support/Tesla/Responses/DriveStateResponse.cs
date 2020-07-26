using Newtonsoft.Json;
using System;

namespace Dashboard.Support.Tesla
{
	public class DriveStateResponse
	{
		public DateTimeOffset GpsTimestampUtc => DateTimeOffset.FromUnixTimeMilliseconds(GpsTimestampUnixTimeSeconds);

		public DateTimeOffset TimestampUtc => DateTimeOffset.FromUnixTimeSeconds(GpsTimestampUnixTimeMilliseconds);
		
		[JsonProperty("gps_as_of")]
		public long GpsTimestampUnixTimeMilliseconds { get; set; }

		[JsonProperty("heading")]
		public int Heading { get; set; }

		[JsonProperty("latitude")]
		public float Latitude { get; set; }

		[JsonProperty("longitude")]
		public float Longitude { get; set; }

		[JsonProperty("native_location_supported")]
		public int IsNativeLocationSupported { get; set; }

		[JsonProperty("native_latitude")]
		public float NativeLatitude { get; set; }
		
		[JsonProperty("native_longitude")]
		public float NativeLongitude { get; set; }

		[JsonProperty("native_type")]
		public string LocationCoordinatesSystem { get; set; }

		[JsonProperty("power")]
		public int Power { get; set; }

		[JsonProperty("shift_state")]
		public string ShiftState { get; set; }

		[JsonProperty("speed")]
		public float? Speed { get; set; }

		[JsonProperty("timestamp")]
		public long GpsTimestampUnixTimeSeconds { get; set; }
	}
}

