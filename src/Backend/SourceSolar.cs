using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Support;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using DataItems;

namespace Functions
{
	public class SourceSolar
    {
		public SourceSolar(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

        [FunctionName("solar")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
			log.LogInformation("Requesting solar data");

			var solarEdgeApiKey = _config["SolarEdgeApiKey"];
			var solarEdgeLocationId = Int32.Parse(_config["SolarEdgeLocationId"]);

			var solarHelper = new SolarHelper(solarEdgeLocationId, solarEdgeApiKey, 60, log);

			var solarData = await solarHelper.GetCurrentPowerAsync();
			var gridDto = new TextDataItem
			{
				Id = "grid_power",
				Label = "Grid (kW)",
				Value = solarData.Grid.CurrentPower.ToString()
			};
			var houseDto = new TextDataItem
			{
				Id = "house_power",
				Label = "House (kW)",
				Value = solarData.House.CurrentPower.ToString()
			};
			var solarDto = new TextDataItem
			{
				Id = "solar_power",
				Label = "PV (kW)",
				Value = solarData.Solar.CurrentPower.ToString()
			};

			var sourceDto = new SourceData
			{
				Id = "solar",
				Title = "Photovoltaik",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = new List<DataItem> {
					gridDto,
					houseDto,
					solarDto
				}
			};

			return new OkObjectResult(sourceDto);
        }
    }
}