using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourcesSupport;
using System.Threading.Tasks;

namespace Tests
{
	[TestClass]
	public class SourcesTests
	{
		[TestMethod]
		public async Task GetWorkoutOfTheDay_ShouldReturnValidWod()
		{
			var wodList = await WodHelpers.GetRawWodAsync(null);
			wodList.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public async Task GetSolarCurrentPower_ShouldReturnValidValues()
		{
			var solarHelper = new SolarHelper(414683, "", null);
			var currentPower = await solarHelper.GetCurrentPowerAsync();
			currentPower.Grid.Status.Should().Be("active");
		}
	}
}
