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
			var wod = await WodHelpers.GetRawWodAsync(null);
			wod.Should().NotBeNullOrWhiteSpace();
		}
	}
}
