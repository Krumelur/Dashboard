using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dashboard.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Client.Shared.SourceCards
{
	public abstract class BaseSourceCard : ComponentBase
	{
		[Inject]
		public HttpClient HttpClient { get; set; }

		[Inject]
		public IConfiguration Configuration { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		protected async Task<SourceHistory> GetSourceHistory(string sourceId, int dataPoints = 1)
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
	}
}