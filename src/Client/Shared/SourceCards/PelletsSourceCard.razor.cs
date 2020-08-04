using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Shared.SourceCards
{
	public partial class PelletsSourceCard : BaseSourceCard
	{
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
	}
}