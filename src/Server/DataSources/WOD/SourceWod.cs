using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Dashboard.Models;
using System.Linq;

namespace Dashboard.Server.DataSources.WOD
{
    public class SourceWod
    {
		public SourceWod(IConfiguration config)
		{
			_config = config;
		}
		readonly IConfiguration _config;

        [FunctionName("GetWodData")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "wod")] HttpRequest req,
            ILogger log)
        {
			log.LogInformation("Requesting WOD data");

			// This function should only run if the ExtendedPermsFunctionsAuthKey is passed in
			// via the x-functions-key header.
			if(!Helpers.CheckFunctionAuthKey(_config["ExtendedPermsFunctionsAuthKey"], req))
			{
				return new BadRequestObjectResult("Failed to verify ExtendedPermsFunctionsAuthKey.");
			}

			// Extract WOD from website.
			var wodHtml = await WodHelper.GetWodContentPhantomJS(_config["PhantomJsApiKey"]);

			var sourceDto = new SourceData
			{
				Id = "wod",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = new [] {
					new DataItem
					{
						Id = $"wod_data",
						Type = DataItemType.Text,
						Label = "WOD",
						Value = wodHtml
					}
				}
			};

			return new OkObjectResult(sourceDto);
        }
    }
}
