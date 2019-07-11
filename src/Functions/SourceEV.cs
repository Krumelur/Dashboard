using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SourcesSupport;
using Microsoft.Extensions.Configuration;
using TeslaApi;
using System.Linq;

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

			var teslaApi = new NativeTeslaApi(teslaClientId, teslaClientSecret, "Dashboard");

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
			
			// TODO: Turn into SourceDTO
			return new OkObjectResult("Vehicle data");
		}
	}
}
