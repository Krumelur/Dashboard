using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components.Web;
using Dashboard.Models;

namespace Client.Shared.SourceCards
{
	public partial class PelletsSourceCard : BaseSourceCard
	{
		public override string SourceId => "pellets";

		public override int NumInitialDataPoints => 30;

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			await UpdateInitialHistory();
			await RefreshUI(InitialHistory);
		}

		async Task RefreshUI(SourceHistory history)
		{
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
				BackgroundColor = new List<string> { ChartColor.FromRgba(226, 228, 242, 0.8f) },
				BorderColor = new List<string> { ChartColor.FromRgba(95, 104, 188, 0.5f) },
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

		protected override async Task InitialHistoryLoaded()
		{
			await RefreshUI(InitialHistory);
		}
	}
}