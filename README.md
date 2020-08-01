# Dashboard

![Deploy API project to Azure Functions](https://github.com/Krumelur/Dashboard/workflows/Deploy%20API%20project%20to%20Azure%20Functions/badge.svg)

![Deploy Blazor Client to Azure](https://github.com/Krumelur/Dashboard/workflows/Deploy%20Blazor%20Client%20to%20Azure/badge.svg)

![Build Harvester for Linux (Raspberry)](https://github.com/Krumelur/Dashboard/workflows/Build%20Harvester%20for%20Linux%20(Raspberry)/badge.svg)

Before breaking changes: https://github.com/Krumelur/Dashboard/tree/e055007854a09320428b1c572688bafaaf839713

## Harvester

### Command line parameters

* `--ignorelastsourceupdate=true/fale`: If specified, harvester ignores the last update time of a source and forces and update even if the source is not due for processing
* `--keyvaultclientid=ID`: Required to access secrets stored in Azure Key Vault. Find the client ID in Azure AD App registration.
* `--keyvaultclientsecret=SECRET`: Required to access secrets stored in Azure Key Vault. Find secret in Azure AD App registration.
* `--ignoresecretsettings=true/false`: By default, development builds are using user secrets and release builds are using Key Vault. If this setting is true, settings will only be read from appsettings.json to allow easier testing.
* `--deactivatedatabaseconnection=true/false`: If true, no connection to the database will be made and thus no updates. Useful for testing.

### Configure sources

The harvester is reading configuration from `appsettings.json`, from user secrets and from Azure Key Vault.

#### General configuration

**Non-sensitive configuration** should go into `appsettings.json` using the following format:

```json
{
    "HarvesterSettings":
    {
    	"HarvesterSchedule": "*/5 * * * *",
    	"ConfiguredSources": ["solar", "wod", "pellets", "tesla"],
    	"KeyVaultUri": "https://krumelurvault.vault.azure.net",
    	"PelletsUnitUri": "http://192.168.178.38:8080"
    }
}
```

* `HarvesterSchedule`: CRON expression defines how often the harvester should check if sources are due (https://crontab.guru is a nice tool to create CRON expressions)
* `ConfiguredSources`: Array of string representing the sources to be polled. The string values will be used to create instances of `Source<STRING_VALUE>` via reflection, for example `solar` becomes `SourceSolar` (casing doesn't matter)
* `KeyVaultUri`: if used in combination with the corresponding command line paramaters, secret/sensitive configuration values will be read from the key vault specified
* `PelletsUnitUri`: URI/IP of the ETA Pellets unit (used for source `pellets`)

All **sensitive configuration** should be placed in a user secrets file `secrets.json` or into Azure Key Vault:

```json
{
	"HarvesterSettings":
	{
		"PhantomJsApiKey" : "API key for Phantom JS service to scrape web content",
		"ExtendedPermsFunctionsAuthKey": "...",
		"StandardPermsFunctionsAuthKey": "...",
		"SolarEdgeApiKey": "API key from SolarEdge monitoring portal",
		"SolarEdgeLocationId": "Location ID from SolarEdge monitoring portal",
		"CosmosDbConnectionString": "Connection string for Cosmos DB to store retrieved source data",
		"TeslaClientId": "Client ID of Tesla API",
		"TeslaClientSecret": "Client secret of Tesla API",
		"TeslaUsername": "Username to access Tesla API",
		"TeslaPassword": "Password to access Tesla API"
	}
}
```

#### Source specific configuration

When executed the first time, the harvester will create a configuration file for every source using the convention `<SOURCE_ID>.json`.
These files will live in `{Environment.SpecialFolder.ApplicationData}/dashboardharvester`.

* On macOS this resolves to `/Users/<USERNAME>/.config/dashboardharvester`
* On Raspbian the folder is located at `/home/pi/.config/dashboardharvester`

```json
{
    "id": "solar",
    "name": "Solar",
    "LastUpdateUtc": null,
    "CronExecutionTime": "*/10 * * * *",
    "NextExecutionDueUtc": null,
    "IsEnabled": true
}
```

* `id`: Identifier of the source. This must match the ID used in `ConfiguredSources`
* `name`: Human readable name of the source
* `LastUpdateUtc`: time when the source was last polled/updated
* `CronExecutionTime`: defines when/how often to poll the source (https://crontab.guru is a nice tool to create CRON expressions)
* `NextExecutionDueUtc`: time and date when the source should be checked the next time
* `IsEnabled`: if `false`, the source will not be polled

### Build release version for Raspberry

The main branch of this repo has an action defined that automatically builds a Linux executable upon a push.
By checking the last build under `https://github.com/Krumelur/Dashboard/actions?query=workflow%3A%22Build+Harvester+for+Linux+%28Raspberry%29%22`the newest ZIP containing the harvester can be downloaded.

**To make the harvester executable, run** `chmod +x Harvester`

For manual builds use:

* Execute `dotnet publish -c Release /p:PublishSingleFile=true -r linux-arm`
* Locate the generated files at `src/Harvester/bin/Release/netcoreapp3.1/linux-arm/publish`
* Copy all files to Raspberry at `/home/pi/Apps/Harvester`

### Run harvester as a service on Raspberry

Information: https://www.raspberrypi.org/documentation/linux/usage/systemd.md

* Under `/etc/systemd/system` create a file called `harvester.service`
* Paste the content below

```bash
[Unit]
Description=Dashboard Harvester
After=multi-user.target

[Service]
Type=simple
ExecStart=/home/pi/Apps/Harvester/Harvester --keyvaultclientid=<client ID from Azure AD> --keyvaultclientsecret=<client secret from Azure AD>
WorkingDirectory=/home/pi/Apps/Harvester
Restart=on-abort
User=pi

[Install]
WantedBy=multi-user.target
```

* Start the service with the command: `sudo systemctl start harvester.service`
* Get the status of the service with the command: `sudo systemctl status harvester.service`
* Stop the service with the command: `sudo systemctl stop harvester.service`

If the service starts and stops as expected and the status shows no errors, run `sudo systemctl enable harvester.service` to launch it automatically upon booting the Raspberry.

## Client

The client app is built with [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) an [Blazorise](https://blazorise.com/) as an abstraction over Bootstrap. It's using the web-assembly (WASM) mode of Blazor and is hosted on an Azure Blob Storage account. In the repo, there's a [GitHub action](https://github.com/Krumelur/Dashboard/actions?query=workflow%3A%22Deploy+Blazor+Client+to+Azure%22) to automatically deploy the main branch upon changes.

Data shown in the client is retrieved from the [Dashboard API](#api).

### Required configuration

* To call into the API, the base URL must be configured using `ApiBaseUrl`
* The access the API, a key named `StandardPermsFunctionsAuthKey` is required. This key is not meant to be a highly sensitive secret but merely to prevent the API from being called by anyone too easily in order to avoid unnecessary cost.

All settings should be stored in `wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://NAME_OF_FUNCTIONS_WEB_APP.azurewebsites.net",
  "StandardPermsFunctionsAuthKey": "<KEY AS CONFIGURED IN API PROJECT>"
}
```

## API

The Dashboard project includes a Azure Functions based API layer which retrieves (historical) source data from the CosmosDB database.

The API gets automatically deployed to Azure using a [GitHub action](https://github.com/Krumelur/Dashboard/actions?query=workflow%3A%22Deploy+API+project+to+Azure+Functions%22).

All endpoints of the API are protected by a key (`AuthorizationLevel.Function`).
The functions web app resource on Azure should configure a "Host" key called `StandardPerms` at host level (Web app project -> "App keys" in the side menu in the portal) which will then be usable for all functions. This key must match the one configured for the [client app](#required-configuration).

The key is not meant to be a highly sensitive secret but merely to prevent the API from being called by anyone too easily in order to avoid unnecessary cost.

API functions that perform security sensitive operations use additional protection. 
