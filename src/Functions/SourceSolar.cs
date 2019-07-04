using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SourcesSupport;
using DTOs;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Functions
{
	public class SourceSolar
	{
		public SourceSolar(IConfiguration config)
		{
			_config = config;
		}
		IConfiguration _config;

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

			// TODO: Turn into SourceDTO
			return new OkObjectResult(solarData);
		}
	}
}
