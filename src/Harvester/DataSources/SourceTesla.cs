using System;
using System.Threading.Tasks;
using Dashboard.Models;
using Harvester;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dashboard.Support.Tesla;

namespace Dashboard.Harvester.DataSources.Tesla
{
    public class SourceTesla : SourceBase
    {
		public SourceTesla(HarvesterSettings harvesterSettings, ILogger logger) : base(harvesterSettings, logger)
		{
		}

        public override async Task<SourceData> GetData()
        {
			_logger.LogInformation("Requesting vehicle data");

			if(string.IsNullOrWhiteSpace(_harvesterSettings.TeslaClientId))
			{
				_logger.LogCritical("Missing Tesla Client ID from configuration!");
				return null;
			}

			if(string.IsNullOrWhiteSpace(_harvesterSettings.TeslaClientSecret))
			{
				_logger.LogCritical("Missing Tesla Client Secret from configuration!");
				return null;
			}

			if(string.IsNullOrWhiteSpace(_harvesterSettings.TeslaUsername))
			{
				_logger.LogCritical("Missing Tesla username from configuration!");
				return null;
			}

			if(string.IsNullOrWhiteSpace(_harvesterSettings.TeslaPassword))
			{
				_logger.LogCritical("Missing Tesla password from configuration!");
				return null;
			}

			var teslaApi = new TeslaApi(_harvesterSettings.TeslaClientId, _harvesterSettings.TeslaClientSecret, "Dashboard", 60);
			var loginResponse = await teslaApi.LoginAsync(_harvesterSettings.TeslaUsername, _harvesterSettings.TeslaPassword);
			var allVehicles = await teslaApi.GetAllVehiclesAsync();
			var vehicle = allVehicles[0];

			// Only get data if car is awake. Don't wake it up because it would have negative impact on battery
			if(vehicle.Status != "online")
			{
				_logger.LogInformation($"Car is not online. Not retrieving data. Status is: '{vehicle.Status}'");
				return new SourceData
				{
					Id = Guid.NewGuid().ToString(),
					SourceId = "tesla",
					TimeStampUtc = DateTimeOffset.UtcNow,
					DataItems = new DataItem[] {
						new DataItem
						{
							Id = "vehicle_state",
							Type = DataItemType.Decimal,
							Label = "Fahrzeugzustand",
							Value = vehicle.Status
						}
					}
				};
			}

			var vehicleData = await teslaApi.GetVehicleDataAsync(vehicle.Id);
			
			var sourceData = new SourceData
			{
				Id = Guid.NewGuid().ToString(),
				SourceId = "tesla",
				TimeStampUtc = DateTimeOffset.UtcNow,
				DataItems = new DataItem[] {
					new DataItem
					{
						Id = "vehicle_state",
						Type = DataItemType.Decimal,
						Label = "Fahrzeugzustand",
						Value = vehicle.Status
					},
					new DataItem
					{
						Id = "inside_temperature",
						Type = DataItemType.Decimal,
						Label = "Temperatur Innenraum (Grad Celsius)",
						Value = vehicleData.ClimateState.InsideTemperature
					},
					new DataItem
					{
						Id = "climate_enabled",
						Type = DataItemType.Boolean,
						Label = "Heizung/Klimaanlage an",
						Value = vehicleData.ClimateState.ClimateEnabled
					},
					new DataItem
					{
						Id = "heading",
						Type = DataItemType.Integer,
						Label = "Fahrtrichtung",
						Value = vehicleData.DriveState.Heading
					},
					new DataItem
					{
						Id = "shift_state",
						Type = DataItemType.Text,
						Label = "Fahrzustand",
						Value = vehicleData.DriveState.ShiftState
					},
					new DataItem
					{
						Id = "latitude",
						Type = DataItemType.Decimal,
						Label = "Breitengrad",
						Value = vehicleData.DriveState.Latitude
					},
					new DataItem
					{
						Id = "longitude",
						Type = DataItemType.Decimal,
						Label = "Längengrad",
						Value = vehicleData.DriveState.Longitude
					},
					new DataItem
					{
						Id = "charge_level_percent",
						Type = DataItemType.Integer,
						Label = "Ladezustand (%)",
						Value = vehicleData.ChargeState.ChargeLevelPercent
					},
					new DataItem
					{
						Id = "charging_state",
						Type = DataItemType.Text,
						Label = "Ladevorgang",
						Value = vehicleData.ChargeState.ChargingState
					},
					new DataItem
					{
						Id = "charge_power",
						Type = DataItemType.Decimal,
						Label = "Ladeleistung (kW)",
						Value = vehicleData.ChargeState.ChargePower
					}
				}
			};

			return sourceData;
        }
    }
}
