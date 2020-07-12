using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Dashboard.Models;

namespace Dashboard.Server.DataSources.SolarEdge
{
    public class SourceSolarEdge
    {
		public SourceSolarEdge(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

        [FunctionName("GetSolarEdgeData")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "solar")] HttpRequest req,
            ILogger log)
        {
			log.LogInformation("Requesting solar data");

			// This function should only run if the ExtendedPermsFunctionsAuthKey is passed in
			// via the x-functions-key header.
			if(!Helpers.CheckFunctionAuthKey(_config["ExtendedPermsFunctionsAuthKey"], req))
			{
				return new BadRequestObjectResult("Failed to verify ExtendedPermsFunctionsAuthKey.");
			}

			var solarEdgeApiKey = _config["SolarEdgeApiKey"];
			var solarEdgeLocationId = Int32.Parse(_config["SolarEdgeLocationId"]);

			var solarHelper = new SolarEdgeHelper(solarEdgeLocationId, solarEdgeApiKey, 60, log);

			var solarData = await solarHelper.GetCurrentPowerAsync();
			
			var gridDto = new DataItem
			{
				Id = "grid_power",
				Type = DataItemType.Decimal,
				Label = "Netz (kW)",
				Value = solarData.Grid.CurrentPower.ToString()
			};

			var houseDto = new DataItem
			{
				Id = "house_power",
				Type = DataItemType.Decimal,
				Label = "Haus (kW)",
				Value = solarData.House.CurrentPower.ToString()
			};

			var solarDto = new DataItem
			{
				Id = "solar_power",
				Type = DataItemType.Decimal,
				Label = "PV (kW)",
				Value = solarData.Solar.CurrentPower.ToString()
			};

			var solarPerformanceDto = new DataItem
			{
				Id = "solar_performance",
				Type = DataItemType.Decimal,
				Label = "Performance (%)",
				Value = Math.Round(solarData.Solar.CurrentPower / 9.8f * 100)
			};

			var sourceDto = new SourceData
			{
				Id = "solar",
				Title = "Photovoltaik",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = new DataItem[] {
					gridDto,
					houseDto,
					solarDto,
					solarPerformanceDto
				}
			};

			return new OkObjectResult(sourceDto);
        }
    }
}
