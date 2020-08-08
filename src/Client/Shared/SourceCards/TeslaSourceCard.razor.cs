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
			await UpdateInitialHistory();
			RefreshUI(InitialHistory);
		}

		void RefreshUI(SourceHistory history)
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
			}
		}

		public int ChargeLevelPercent { get; set; }

		public bool IsCarOffline { get; set; }

		protected override async Task InitialHistoryLoaded()
		{
			IsCarOffline = false;
			RefreshUI(InitialHistory);
			await JSRuntime.InvokeVoidAsync("updateBingMap", 47.855042, 12.185040);
		}
	}
}