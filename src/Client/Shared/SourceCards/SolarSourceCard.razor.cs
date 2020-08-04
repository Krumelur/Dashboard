﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise.Charts;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Shared.SourceCards
{
	public partial class SolarSourceCard : BaseSourceCard
	{
		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			var history = await GetSourceHistory("solar", 6);

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
				BackgroundColor = new List<string> { ChartColor.FromRgba(243, 120, 121, 0.5f) },
				BorderColor = new List<string> { ChartColor.FromRgba(238, 64, 64, 0.5f) },
				Fill = true,
				PointRadius = 2,
				BorderDash = new List<int> { }
			};
			
			var dataSetSolarPower = new LineChartDataset<double>
			{
				Label = productionEnergy,
				Data = dataSolarPower,
				Fill = true,
				BackgroundColor = new List<string> { ChartColor.FromRgba(97, 201, 137, 0.5f) },
				BorderColor = new List<string> { ChartColor.FromRgba(28, 180, 91, 0.5f) },
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

		protected override async Task OnInitializedAsync()
		{
			await HandleRefreshClick(null);
		}
	}
}