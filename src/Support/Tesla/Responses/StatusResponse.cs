using Newtonsoft.Json;

namespace Dashboard.Support.Tesla
{
	/// <summary>
	/// Generic status response
	/// </summary>
	public class StatusResponse
	{
		[JsonProperty("result")]
		public bool Success { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }

		public override string ToString() => $"[{nameof(StatusResponse)}] Success = {Success}; Reason = {Reason}";
	}
}
