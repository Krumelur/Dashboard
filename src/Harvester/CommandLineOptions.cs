using CommandLine;

public class CommandLineOptions
{
	[Option("ignorelastsourceupdate", Default = false, Required = false, HelpText = "If specified, harvester ignores the last update time of a source and forces and update even if the source is not due for processing.")]
	public bool IgnoreLastSourceUpdate { get; set; } = false;

	[Option("keyvaultclientid", Default = null, Required = false, HelpText = "Required to access secrets stored in Azure Key Vault. Usage: '--keyvaultclientid=clientid' (find client ID in Azure AD App registration)")]
	public string KeyVaultClientId { get; set; }
	
	[Option("keyvaultclientsecret", Default = null, Required = false, HelpText = "Required to access secrets stored in Azure Key Vault. Usage: '--keyvaultclientsecret=secret' (find secret in Azure AD App registration)")]
	public string KeyVaultClientSecret { get; set; }

	[Option("ignoresecretsettings", Default = null, Required = false, HelpText = "By default, development builds are using user secrets and release builds are using Key Vault. If this setting is true, settings will only be read from appsettings.json.")]
	public bool IgnoreSecretSettings { get; set; }

	[Option("deactivatedatabaseconnection", Default = false, Required = false, HelpText = "If true, no connection to the database will be made and thus no updates. Useful for testing.")]
	public bool DeactivateDatabaseConnection { get; set; }

	/*
	[Usage(ApplicationAlias = "harvester")]
	public static IEnumerable<Example> Examples
	{
		get
		{
			return new List<Example>() {
						new Example("Write a single line of text to the output file", new Options { OutputFilename = "outputfile.txt", Text = "This will end up in the file" }),
					};
		}
	}
	*/
}