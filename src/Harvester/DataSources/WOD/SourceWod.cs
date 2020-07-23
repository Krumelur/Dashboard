
using System;
using System.Threading.Tasks;
using Dashboard.Models;
using Harvester;
using Microsoft.Extensions.Logging;

namespace Dashboard.Harvester.DataSources.WOD
{
	public class SourceWod : SourceBase
	{
		public SourceWod(HarvesterSettings harvesterSettings, ILogger logger) : base(harvesterSettings, logger)
		{
		}

		public override async Task<SourceData> GetData()
		{
			_logger.LogInformation("Requesting WOD data");

			if(string.IsNullOrWhiteSpace(_harvesterSettings.PhantomJsApiKey))
			{
				_logger.LogCritical("Missing PhantomJS API key from configuration!");
				return null;
			}

			// Extract WOD from website.
			var wodHtml = await WodHelper.GetWodContentPhantomJS(_harvesterSettings.PhantomJsApiKey);

			var sourceDto = new SourceData
			{
				Id = Guid.NewGuid().ToString(),
				SourceId = "wod",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = new[] {
					new DataItem
					{
						Id = $"wod_data",
						Type = DataItemType.Text,
						Label = "WOD",
						Value = wodHtml
					}
				}
			};

			return sourceDto;
		}
	}
}
