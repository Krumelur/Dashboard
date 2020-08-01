using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dashboard.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Client.Shared
{
	public partial class WodSourceCard : ComponentBase
	{
		[Inject]
		HttpClient HttpClient { get; set; }

		[Inject]
		IConfiguration Configuration {get; set; }

		public string WodHtml { get; set; }

		async Task<SourceHistory> GetSourceHistory(string sourceId, int dataPoints = 1)
		{
			var authKey = Configuration["StandardPermsFunctionsAuthKey"];
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
			var history = await GetSourceHistory("wod");
			if(history?.HistoryData != null && history.HistoryData[0]?.DataItems != null)
			{
				WodHtml = history.HistoryData[0].DataItems[0].Value.ToString();
			}
			else
			{
				WodHtml = "Leider kein WOD gefunden :-(";
			}
		}
		protected override async Task OnInitializedAsync()
		{
			await HandleRefreshClick(null);
		}
	}
}