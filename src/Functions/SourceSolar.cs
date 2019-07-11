using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SourcesSupport;
using Microsoft.Extensions.Configuration;
using DTOs;

namespace Functions
{
	public class SourceSolar
	{
		public SourceSolar(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

		[FunctionName("GetSolar")]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "solar")] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("Requesting solar data");

			var solarEdgeApiKey = _config["SolarEdgeApiKey"];
			var solarEdgeLocationId = Int32.Parse(_config["SolarEdgeLocationId"]);

			var solarHelper = new SolarHelper(solarEdgeLocationId, solarEdgeApiKey, log);

			var solarData = await solarHelper.GetCurrentPowerAsync();
			var gridDto = new TextDataDTO {
				Id = "GRID_POWER",
				Label = "Grid power (kW)",
				Value = solarData.Grid.CurrentPower.ToString()
			};
			var houseDto = new TextDataDTO
			{
				Id = "HOUSE_POWER",
				Label = "House power (kW)",
				Value = solarData.House.CurrentPower.ToString()
			};
			var solarDto = new TextDataDTO
			{
				Id = "SOLAR_POWER",
				Label = "Solar power (kW)",
				Value = solarData.Solar.CurrentPower.ToString()
			};

			var sourceDto = new SourceDTO
			{
				Id = "SOLAR",
				Title = "Solar panel",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = {
					gridDto,
					houseDto,
					solarDto
				}
			};

			return new OkObjectResult(sourceDto);
		}
	}
}
