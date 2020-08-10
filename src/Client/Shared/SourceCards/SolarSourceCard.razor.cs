using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components.Web;
using Dashboard.Models;

namespace Client.Shared.SourceCards
{
	public partial class SolarSourceCard : BaseSourceCard
	{
		public override string SourceId => "solar";

		public override int NumInitialDataPoints => 6;

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			await UpdateInitialHistory();
			await RefreshUI(InitialHistory);
		}

		async Task RefreshUI(SourceHistory history)
		{
			var labels = history.HistoryData
				.Select(d => d.TimeStampUtc.LocalDateTime.ToString("HH:mm"))
				.Reverse()
				.ToArray();

			var dataHousePower = history.HistoryData
				.Select(d => {
					string value = d.DataItems[1].Value.ToString();
					double.TryParse(value, out double power);
					return power;
				})
				.Reverse()
				.ToList();

			var dataSolarPower = history.HistoryData
				.Select(d => {
					string value = d.DataItems[2].Value.ToString();
					double.TryParse(value, out double power);
					return power;
				})
				.Reverse()
				.ToList();

			var productionEnergy = $"PV ({dataSolarPower.Last():0.#}kW)";
			var consumptionEnergy = $"Verbrauch ({dataHousePower.Last():0.#}kW)";
			
			var dataSetHousePower = new LineChartDataset<double>
			{
				Label = consumptionEnergy,
				Data = dataHousePower,
				BackgroundColor = new List<string> { ChartColor.FromRgba(249, 134, 134, 0.5f) },
				BorderColor = new List<string> { ChartColor.FromRgba(249, 134, 134, 1f) },
				Fill = true,
				PointRadius = 2,
				BorderDash = new List<int> { }
			};
			
			var dataSetSolarPower = new LineChartDataset<double>
			{
				Label = productionEnergy,
				Data = dataSolarPower,
				Fill = true,
				BackgroundColor = new List<string> { ChartColor.FromRgba(226, 228, 242, 0.8f) },
				BorderColor = new List<string> { ChartColor.FromRgba(95, 104, 188, 0.5f) },
				PointRadius = 2,
				BorderDash = new List<int> { }
			};

			
			await lineChart.Clear();
			await lineChart.AddLabel(labels);
			await lineChart.AddDataSet(dataSetHousePower);
			await lineChart.AddDataSet(dataSetSolarPower);
			await lineChart.Update();
		}

		protected LineChart<double> lineChart;

		protected override async Task InitialHistoryLoaded()
		{
			await RefreshUI(InitialHistory);
		}
	}
}