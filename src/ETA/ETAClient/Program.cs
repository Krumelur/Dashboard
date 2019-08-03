using ETASupport;
using System;
using System.Threading.Tasks;

namespace ETAClient
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var pollDelayHours = 1 / 60f / 60f;
			var pelletsUnitBaseUrl = "http://192.168.178.22";

			Console.WriteLine($"Starting to poll pellets unit at {pelletsUnitBaseUrl}. Configured delay is {pollDelayHours} hours.");
			var comm = new Communication(pelletsUnitBaseUrl);

			try
			{
				while (true)
				{
					await Task.Delay(TimeSpan.FromHours(pollDelayHours));
					var pelletsData = await comm.GetPelletsDataAsync();
					Console.WriteLine($"Received pellets data: {pelletsData}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Stopped polling pellets unit: {ex}");
				throw;
			}
		}
	}
}
