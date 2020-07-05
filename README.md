# Dashboard

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


 