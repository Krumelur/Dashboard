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

namespace Client.Shared.SourceCards
{
	public partial class PelletsSourceCard : ComponentBase
	{
		[Inject]
		HttpClient HttpClient { get; set; }

		[Inject]
		IConfiguration Configuration {get; set; }

		async Task<SourceHistory> GetSourceHistory(string sourceId, int dataPoints = 1)
		{
			var authKey = Configuration["FunctionsAuthKey"];
			var baseUrl = Configuration["ApiBaseUrl"];

			var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/sourcedata/{sourceId}?numDataPoints={dataPoints}");
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
			var history = await GetSourceHistory("pellets", 30);

			var labels = history.HistoryData
				.Select(d => d.TimeStampUtc.LocalDateTime.ToString("d.M."))
				.Reverse()
				.ToArray();

			var dataSupplies = history.HistoryData
				.Select(d => {
					// Item 0 = supplies in kg (label "supplies")
					string value = d.DataItems[0].Value.ToString();
					double.TryParse(value, out double supplies);
					return supplies;
				})
				.Reverse()
				.ToList();

			var currentSupplies = $"Vorrat: {dataSupplies.Last():0.0}kg";
			
			var dataSetPelletSupplies = new LineChartDataset<double>
			{
				Label = currentSupplies,
				Data = dataSupplies,
				BackgroundColor = new List<string> { ChartColor.FromRgba(243, 120, 121, 0.5f) },
				BorderColor = new List<string> { ChartColor.FromRgba(238, 64, 64, 0.5f) },
				Fill = true,
				PointRadius = 2,
				BorderDash = new List<int> { }
			};
			
			await lineChart.Clear();
			await lineChart.AddLabel(labels);
			await lineChart.AddDataSet(dataSetPelletSupplies);
			await lineChart.Update();
		}

		protected LineChart<double> lineChart;

		protected override async Task OnInitializedAsync()
		{
			await HandleRefreshClick(null);
		}

		// protected override async Task OnAfterRenderAsync(bool firstRender)
		// {
			
		// }
	}
}