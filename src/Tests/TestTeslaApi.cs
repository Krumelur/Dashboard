using System;
using System.Threading.Tasks;
using Dashboard.Support.Tesla;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class TestTeslaApi
	{
		[TestInitialize]
		public async Task InitTests()
		{
			var builder = new ConfigurationBuilder()
				.AddUserSecrets<TestTeslaApi>();

			// Build now because we need configuration from appsettings.json to build the configuration for key vault.
			_configuration = builder.Build();
			_teslaApi = new TeslaApi(_configuration["HarvesterSettings:TeslaClientId"], _configuration["HarvesterSettings:TeslaClientSecret"], "Dashboard", 60);
			var loginResponse = await _teslaApi.LoginAsync(_configuration["HarvesterSettings:TeslaUsername"], _configuration["HarvesterSettings:TeslaPassword"]);
			var allVehicles = await _teslaApi.GetAllVehiclesAsync();
			_vehicle = allVehicles[0];
		}

		IConfiguration _configuration;
		TeslaApi _teslaApi;
		VehicleResponse _vehicle;

		[TestMethod]
		public void Vehicle_distinguish_sleep_and_awake_correct()
		{
			Console.WriteLine($"Vehicle status: {_vehicle.Status}");

			Assert.IsNotNull(_vehicle.Status, "Vehicle state is NULL but should be 'online', 'offline' or 'asleep'.");
			//var vehicleState = await teslaApi.GetVehicleStateAsync(vehicle.Id);
		}

		[TestMethod]
		public async Task Vehicle_get_information_successful()
		{
			if(_vehicle.Status != "online")
			{
				int wakeUpCounter = 4;
				while(true)
				{
					var wakeUpResponse = await _teslaApi.WakeUpVehicleAsync(_vehicle.Id, 10);
					await Task.Delay(TimeSpan.FromSeconds(5));
					if(wakeUpResponse.Status == "online")
					{
						break;
					}
					wakeUpCounter--;
					if(wakeUpCounter < 0)
					{
						throw new Exception("Failed to wake up car");
					}
				}
			}

			var vehicleData = await _teslaApi.GetVehicleDataAsync(_vehicle.Id);
			Assert.IsNotNull(vehicleData);
		}
	}

}
