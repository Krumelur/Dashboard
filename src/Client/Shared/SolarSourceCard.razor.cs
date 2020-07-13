using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dashboard.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;

namespace DashboardClient.Components.Shared
{
	public partial class SolarSourceCard : ComponentBase
	{
		[Inject] HttpClient HttpClient { get; set; }

		public async Task HandleRefreshClick(MouseEventArgs args)
		{
			var url = "https://krumelurdashboardapi.azurewebsites.net/api/sourcedata/solar?numDataPoints=6";

			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Add("x-functions-key", "$tandardk3y");

				var response = await HttpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();
				var historyData = JsonConvert.DeserializeObject<SourceData>(responseContent);
				Console.WriteLine(responseContent);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to get source data: " + ex);
				throw;
			}
		}
	}
}