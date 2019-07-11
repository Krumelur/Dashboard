using Newtonsoft.Json;

namespace TeslaApi.Responses
{
	/// <summary>
	/// List of vehicles
	/// </summary>
	public class VehicleListResponse
	{
		[JsonProperty("response")]
		public VehicleResponse[] Vehicles { get; set; }

		[JsonProperty("count")]
		public int Count { get; set; }
	}

}
