using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Blazorise.Charts;
using Dashboard.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Client.Shared
{
	public partial class TeslaSourceCard : ComponentBase
	{
		[Inject]
		HttpClient HttpClient { get; set; }

		[Inject]
		IConfiguration Configuration {get; set; }

		async Task<SourceHistory> GetSourceHistory(string sourceId, int dataPoints = 1)
		{
			var authKey = Configuration["StandardPermsFunctionsAuthKey"];
			var request = new HttpRequestMessage(HttpMethod.Get, $"https://krumelurdashboardapi.azurewebsites.net/api/sourcedata/{sourceId}?numDataPoints={dataPoints}");
			request.Headers.Add("x-functions-key", authKey);

			try
			{
				var response = await HttpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();
				var historyData = JsonConvert.DeserializeObject<SourceHistory>(responseContent);
				return historyData;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to get source data: " + ex);
				throw;
			}
		}

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			var history = await GetSourceHistory("tesla", 1);

			var vehicleStateItem = history.HistoryData[0].DataItems.First(x => x.Id == "vehicle_state");
			var vehicleState = Convert.ToString(vehicleStateItem.Value);

			if(vehicleState != "online")
			{
				IsChartHidden = true;
			}
			else
			{
				IsChartHidden = false;

				var chargeLevelPercentItem = history.HistoryData[0].DataItems.First(x => x.Id == "charge_level_percent");
				int chargeLevelPercent = Convert.ToInt32(chargeLevelPercentItem.Value);

				var dataSetCharge = new DoughnutChartDataset<int>
				{
					Label = $"Ladezustand: {chargeLevelPercent}%",
					Data = new List<int> { chargeLevelPercent, 100 - chargeLevelPercent },
					BackgroundColor = new List<string> { ChartColor.FromRgba(97, 201, 137, 1.0f), ChartColor.FromRgba(0, 0, 0, 0.1f) },
					BorderColor = new List<string> { ChartColor.FromRgba(28, 180, 91, 1.0f), ChartColor.FromRgba(0, 0, 0, 0.1f) },
				};
				
				await doughnutChart.Clear();
				await doughnutChart.AddLabel($"{chargeLevelPercent}% geladen"); //, $"{100 - chargeLevelPercent}% verbleibend");
				await doughnutChart.AddDataSet(dataSetCharge);
				await doughnutChart.Update();
			}
		}

		public bool IsChartHidden { get; set; }

		protected DoughnutChart<int> doughnutChart;

		protected override async Task OnInitializedAsync()
		{
			IsChartHidden = false;
			await HandleRefreshClick(null);
		}

		// protected override async Task OnAfterRenderAsync(bool firstRender)
		// {
			
		// }
	}
}