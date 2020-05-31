# Dashboard

Repository before breaking changes: https://github.com/Krumelur/Dashboard/tree/e055007854a09320428b1c572688bafaaf839713

## Harvester

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
    "CosmosDbConnectionString": "AccountEndpoint=https://..."
}

## Sources

### Required configuration settings

Regular:
none

Secrets:
{
    "SolarEdgeApiKey": "8H...",
    "SolarEdgeLocationId": "4...",
 }