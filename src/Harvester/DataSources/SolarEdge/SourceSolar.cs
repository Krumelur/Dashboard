using System;
using System.Threading.Tasks;
using Dashboard.Models;
using Harvester;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dashboard.Harvester.DataSources.SolarEdge
{
    public class SourceSolar : SourceBase
    {
		public SourceSolar(HarvesterSettings harvesterSettings, ILogger logger) : base(harvesterSettings, logger)
		{
		}

        public override async Task<SourceData> GetData()
        {
			_logger.LogInformation("Requesting solar data");

			var solarEdgeApiKey = _harvesterSettings.SolarEdgeApiKey;
			
			if(string.IsNullOrWhiteSpace(solarEdgeApiKey))
			{
				_logger.LogCritical("Missing Solar Edge API key from configuration!");
				return null;
			}

			if(!Int32.TryParse(_harvesterSettings.SolarEdgeLocationId, out int solarEdgeLocationId))
			{
				_logger.LogCritical("Missing Solar Edge Location ID from configuration or non-numeric value!");
				return null;
			}

			var solarHelper = new SolarEdgeHelper(solarEdgeLocationId, solarEdgeApiKey, 60, _logger);

			var solarData = await solarHelper.GetCurrentPowerAsync();
			
			var gridDto = new DataItem
			{
				Id = "grid_power",
				Type = DataItemType.Decimal,
				Label = "Netz (kW)",
				Value = solarData.Grid.CurrentPower
			};

			var houseDto = new DataItem
			{
				Id = "house_power",
				Type = DataItemType.Decimal,
				Label = "Haus (kW)",
				Value = solarData.House.CurrentPower
			};

			var solarDto = new DataItem
			{
				Id = "solar_power",
				Type = DataItemType.Decimal,
				Label = "PV (kW)",
				Value = solarData.Solar.CurrentPower
			};

			var solarPerformanceDto = new DataItem
			{
				Id = "solar_performance",
				Type = DataItemType.Decimal,
				Label = "Performance (%)",
				Value = Math.Round(solarData.Solar.CurrentPower / 9.8f * 100)
			};

			var sourceDto = new SourceData
			{
				Id = Guid.NewGuid().ToString(),
				SourceId = "solar",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = new DataItem[] {
					gridDto,
					houseDto,
					solarDto,
					solarPerformanceDto
				}
			};

			return sourceDto;
        }
    }
}
