# Dashboard

![Deploy API project to Azure Functions](https://github.com/Krumelur/Dashboard/workflows/Deploy%20API%20project%20to%20Azure%20Functions/badge.svg)

![Deploy Blazor Client to Azure](https://github.com/Krumelur/Dashboard/workflows/Deploy%20Blazor%20Client%20to%20Azure/badge.svg)

![Build Harvester for Linux (Raspberry)](https://github.com/Krumelur/Dashboard/workflows/Build%20Harvester%20for%20Linux%20(Raspberry)/badge.svg)

Before breaking changes: https://github.com/Krumelur/Dashboard/tree/e055007854a09320428b1c572688bafaaf839713

Build release version for Raspberry:

- Execute `dotnet publish -c Release /p:PublishSingleFile=true -r linux-arm`
- Locate the generated files at `src/Harvester/bin/Release/netcoreapp3.1/linux-arm/publish`
- Copy all files to Raspberry at `/home/pi/Apps/Harvester`
- Run as a service on Raspberry (https://www.raspberrypi.org/documentation/linux/usage/systemd.md):
  - Under `/etc/systemd/system` create a file called `harvester.service`
  - Paste the content below

```bash
[Unit]
Description=Dashboard Harvester
After=multi-user.target
 
[Service]
Type=simple
ExecStart=/home/pi/Apps/Harvester/Harvester --whatever=params
WorkingDirectory=/home/pi/Apps/Harvester
Restart=on-abort
User=pi
 
[Install]
WantedBy=multi-user.target
```

## Server

### Required configuration settings

Regular:
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "HarvesterUrl": "http://127.0.0.1:7072/api/harvestconfiguredsources",
    "HarvesterSchedule": "0 */5 * * * *"
  }
}

Secrets:
{
    "PhantomJsApiKey" : "...",
    "ExtendedPermsFunctionsAuthKey": "...",
    "StandardPermsFunctionsAuthKey": "...",
    "CosmosDbConnectionString": "AccountEndpoint=https://...",
    "SolarEdgeApiKey": "8H...",
    "SolarEdgeLocationId": "4...",
}

## Storage emulator

To run locally, you'll need the Azurite Storage Emulator.
https://docs.microsoft.com/azure/storage/common/storage-use-azurite

Make sure to start the emulator (Command palette -> Azurite: Start) before debugging the functions' project.
