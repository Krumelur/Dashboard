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

namespace Functions
{
    public static class Function1
    {
        [FunctionName("GetWod")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "wod")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Requesting WOD");

			// Extract WOD from website.
			var rawWod = await WodHelpers.GetRawWodAsync(log);

			return new OkObjectResult("Done");
        }
    }
}
