using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourcesSupport;
using System;
using System.Threading.Tasks;

namespace Tests
{
	[TestClass]
	public class SourcesTests
	{
		[TestInitialize]
		public void InitializeTests()
		{
			var builder = new ConfigurationBuilder();
			// TODO: anything specific needed when running tests on Azure DevOps? 
			//       Does this have to be removed?
			builder.AddUserSecrets<SourcesTests>();
			var config = builder.Build();
			_solarEdgeApiKey = config["SolarEdgeApiKey"];
			_solarEdgeLocationId = Int32.Parse(config["SolarEdgeLocationId"]);
		}

		int _solarEdgeLocationId;
		string _solarEdgeApiKey;

		[TestMethod]
		public async Task GetWorkoutOfTheDay_ShouldReturnValidWod()
		{
			var wodList = await WodHelpers.GetRawWodAsync(null);
			wodList.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public async Task GetSolarCurrentPower_ShouldReturnValidValues()
		{
			var solarHelper = new SolarHelper(_solarEdgeLocationId, _solarEdgeApiKey, null);
			var currentPower = await solarHelper.GetCurrentPowerAsync();
			currentPower.Grid.Status.Should().Be("Active");
		}
	}
}
