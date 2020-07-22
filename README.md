# Dashboard

![Deploy Client to Azure](https://github.com/Krumelur/Dashboard/workflows/Deploy%20Client%20to%20Azure/badge.svg?branch=master)

![Deploy Server Backend Functions to Azure](https://github.com/Krumelur/Dashboard/workflows/Deploy%20Server%20Backend%20Functions%20to%20Azure/badge.svg?branch=master)

Before breaking changes: https://github.com/Krumelur/Dashboard/tree/e055007854a09320428b1c572688bafaaf839713

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
