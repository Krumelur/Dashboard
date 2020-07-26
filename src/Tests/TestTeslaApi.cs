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
        [TestMethod]
        public async Task Vehicle_distinguish_sleep_and_awake_correct()
        {
			var builder = new ConfigurationBuilder()
				.AddUserSecrets<TestTeslaApi>();
			
			// Build now because we need configuration from appsettings.json to build the configuration for key vault.
			var configuration = builder.Build();

			var teslaApi = new TeslaApi(configuration["TeslaClientId"], configuration["TeslaClientSecret"], "Dashboard", 60);
			var loginResponse = await teslaApi.LoginAsync(configuration["TeslaUsername"], configuration["TeslaPassword"]);
			var allVehicles = await teslaApi.GetAllVehiclesAsync();
			var vehicle = allVehicles[0];
			
			Console.WriteLine($"Vehicle status: {vehicle.Status}");
			
			Assert.IsNotNull(vehicle.Status, "Vehicle state is NULL but should be 'online', 'offline' or 'asleep'.");
			//var vehicleState = await teslaApi.GetVehicleStateAsync(vehicle.Id);
		}
    }
}
