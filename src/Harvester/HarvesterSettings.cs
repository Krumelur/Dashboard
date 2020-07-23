namespace Harvester
{
	public class HarvesterSettings
	{
		public string KeyVaultUri { get; set; }
		public string HarvesterSchedule { get; set; }
		public string[] ConfiguredSources { get; set; }

		public string PhantomJsApiKey { get; set; }

		public string SolarEdgeApiKey { get; set; }

		public string SolarEdgeLocationId { get; set; }

		// Put this in a user secrets file. Don't store it in local.settings.json!
		public string CosmosDbConnectionString {get; set; }

		public string PelletsUnitUri { get; set; }
	}
}