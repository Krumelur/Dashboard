using System;
using System.Linq;
using System.Threading.Tasks;
using Dashboard.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Client.Shared.SourceCards
{
	public partial class TeslaSourceCard : BaseSourceCard
	{
		public override string SourceId => "tesla";

		public override int NumInitialDataPoints => 1;

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			TimeStampSource = "(aktualisieren)";
			await UpdateInitialHistory();
			await RefreshUI(InitialHistory);
		}

		async Task RefreshUI(SourceHistory history)
		{
			// We're only retrieving one item.
			var singleSourceData = history.HistoryData[0];
			var vehicleStateItem = singleSourceData.DataItems.First(x => x.Id == "vehicle_state");
			var vehicleState = Convert.ToString(vehicleStateItem.Value);

			if(vehicleState != "online")
			{
				IsCarOffline = true;
			}
			else
			{
				IsCarOffline = false;

				var chargeLevelPercentItem = history.HistoryData[0].DataItems.First(x => x.Id == "charge_level_percent");
				ChargeLevelPercent = Convert.ToInt32(chargeLevelPercentItem.Value);

				var carTemperatureItem = history.HistoryData[0].DataItems.First(x => x.Id == "inside_temperature");
				CarTemperature = $"{Math.Round(Convert.ToDouble(carTemperatureItem.Value))}°";
			}

			await JSRuntime.InvokeVoidAsync("updateBingMap", 47.855042, 12.185040);
		}

		public int ChargeLevelPercent { get; set; }

		public string CarTemperature { get; set; }

		public bool IsCarOffline { get; set; }

		protected override async Task InitialHistoryLoaded()
		{
			IsCarOffline = false;
			await RefreshUI(InitialHistory);
		}
	}
}