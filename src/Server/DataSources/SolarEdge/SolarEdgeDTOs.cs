using Newtonsoft.Json;

namespace Dashboard.Server.DataSources.SolarEdge
{
	public class CurrentPowerFlowRoot
	{
		[JsonProperty("siteCurrentPowerFlow")]
		public CurrentPowerFlow CurrentPowerFlow { get; set; }
	}

	public class CurrentPowerFlow
	{
		[JsonProperty("updateRefreshRate")]
		public int UpdateRefreshRate { get; set; }
		[JsonProperty("unit")]
		public string Unit { get; set; }
		public Connection[] Connections { get; set; }
		[JsonProperty("GRID")]
		public PowerData Grid { get; set; }
		[JsonProperty("LOAD")]
		public PowerData House { get; set; }
		[JsonProperty("PV")]
		public PowerData Solar { get; set; }
	}

	public class PowerData
	{
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("currentPower")]
		public float CurrentPower { get; set; }
	}

	public class Connection
	{
		[JsonProperty("from")]
		public string From { get; set; }
		[JsonProperty("to")]
		public string To { get; set; }
	}
}
