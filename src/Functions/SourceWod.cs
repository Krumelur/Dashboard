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
    public static class Function1
    {
        [FunctionName("GetWod")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "wod")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Requesting WOD");

			// Extract WOD from website.
			var textWods = await WodHelpers.GetRawWodAsync(log);

			var dataItems = textWods
				.Select<string, BaseDataDTO>(w => new TextDataDTO {
					Id = $"WOD_PART_{textWods.IndexOf(w) + 1}",
					Label = $"Part {textWods.IndexOf(w) + 1}",
					Value = w,
				})
				.ToList();

			var result = new SourceDTO
			{
				Id = "WOD",
				Title = "Workout of the day",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = dataItems
			};

			return new OkObjectResult(result);
        }
    }
}
