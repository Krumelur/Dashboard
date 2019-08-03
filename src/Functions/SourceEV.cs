using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TeslaApi;
using System.Linq;
using DTOs;
using System;
using System.Collections.Generic;

namespace Functions
{
	public class SourceEV
	{
		public SourceEV(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

		[FunctionName("GetEV")]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ev")] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("Requesting EV data");

			var teslaClientId = _config["TeslaClientId"];
			var teslaClientSecret = _config["TeslaClientSecret"];
			var username = _config["TeslaUsername"];
			var password = _config["TeslaPassword"];

			if (string.IsNullOrWhiteSpace(teslaClientId)
				|| string.IsNullOrWhiteSpace(teslaClientSecret)
				|| string.IsNullOrWhiteSpace(username)
				|| string.IsNullOrWhiteSpace(password))
			{
				return new BadRequestObjectResult("Login credentials are not configured correctly.");
			}

			var teslaApi = new NativeApi(teslaClientId, teslaClientSecret, "Dashboard", 60);

			var loginResponse = await teslaApi.LoginAsync(username, password);
			if (!string.IsNullOrWhiteSpace(loginResponse.AccessToken))
			{
				log.LogInformation($"Successfully logged in to Tesla API - token expires in [{loginResponse.ExpiresIn.TotalHours} hours].");
			}

			var vehicles = await teslaApi.GetAllVehiclesAsync();
			var firstVehicle = vehicles.FirstOrDefault();
			if (firstVehicle == null)
			{
				return new BadRequestObjectResult("Failed to get vehicle.");
			}

			await teslaApi.WakeUpVehicleAsync(firstVehicle.Id);
			var vehicleData = await teslaApi.GetVehicleDataAsync(firstVehicle.Id);

			var dataItems = new List<DataItem>
			{
				new TextDataItem {
					Id = "CHARGING_STATE",
					Label = "Charging state",
					Value = vehicleData.ChargeState.ChargingSate
				},

				new TextDataItem {
					Id = "CHARGE_LEVEL",
					Label = "Charge level",
					Value = vehicleData.ChargeState.ChargeLevelPercent + "%"
				}
			};

			var sourceData = new SourceData {
				Id = "EV",
				TimeStampUtc = DateTimeOffset.UtcNow,
				Title = "Tesla",
				DataItems = dataItems
			};

			return new OkObjectResult(sourceData);
		}
	}
}
