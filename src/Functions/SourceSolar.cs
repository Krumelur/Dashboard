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

namespace Functions
{
    public static class SourceSolar
    {
        [FunctionName("GetSolar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "solar")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Requesting solar data");

			var solarData = await SolarHelper.GetCurrentPowerAsync(log);

			return OkObjectResult("Solar");
        }
    }
}
