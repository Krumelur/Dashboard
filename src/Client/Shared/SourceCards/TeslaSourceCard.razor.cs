using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Client.Shared.SourceCards
{
	public partial class TeslaSourceCard : BaseSourceCard
	{
		public string TimeStampSource { get; set; }
		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			var history = await GetSourceHistory("tesla", 1);

			// We're only retrieving one item.
			var singleSourceData = history.HistoryData[0];
			TimeStampSource = singleSourceData.TimeStampUtc.ToLocalTime().ToString("g");
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

		protected override async Task OnInitializedAsync()
		{
			IsCarOffline = false;
			await HandleRefreshClick(null);

			await JSRuntime.InvokeVoidAsync("updateBingMap", 47.855042, 12.185040);
    	}

		// protected override async Task OnAfterRenderAsync(bool firstRender)
		// {
			
		// }
	}
}