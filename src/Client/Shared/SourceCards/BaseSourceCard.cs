using System;
using System.Collections.Generic;
using System.Linq;
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

		public abstract string SourceId {get;}

		public abstract int NumInitialDataPoints {get;}

		public SourceHistory InitialHistory {get; set;}

		public string TimeStampSource { get; set; }

		Dictionary<string, string> _mockData = new Dictionary<string, string>
		{
			["solar"] = "{\"sourceId\":\"solar\",\"historyData\":[{\"id\":\"b72ac012-ddfd-43a4-98bc-c5112fbc591f\",\"sourceid\":\"solar\",\"timeStampUtc\":\"2020-08-08T12:20:02.0865602+00:00\",\"dataItems\":[{\"id\":\"grid_power\",\"type\":2,\"label\":\"Netz (kW)\",\"value\":4.63,\"isSensitive\":false},{\"id\":\"house_power\",\"type\":2,\"label\":\"Haus (kW)\",\"value\":0.89,\"isSensitive\":false},{\"id\":\"solar_power\",\"type\":2,\"label\":\"PV (kW)\",\"value\":5.52,\"isSensitive\":false},{\"id\":\"solar_performance\",\"type\":2,\"label\":\"Performance (%)\",\"value\":56,\"isSensitive\":false}]},{\"id\":\"1d86ff5f-8611-4598-b518-35e0ea43ff17\",\"sourceid\":\"solar\",\"timeStampUtc\":\"2020-08-08T12:10:01.1638189+00:00\",\"dataItems\":[{\"id\":\"grid_power\",\"type\":2,\"label\":\"Netz (kW)\",\"value\":4.83,\"isSensitive\":false},{\"id\":\"house_power\",\"type\":2,\"label\":\"Haus (kW)\",\"value\":0.76,\"isSensitive\":false},{\"id\":\"solar_power\",\"type\":2,\"label\":\"PV (kW)\",\"value\":5.59,\"isSensitive\":false},{\"id\":\"solar_performance\",\"type\":2,\"label\":\"Performance (%)\",\"value\":57,\"isSensitive\":false}]},{\"id\":\"bee3ab0c-8901-4318-8244-f423ad7a278e\",\"sourceid\":\"solar\",\"timeStampUtc\":\"2020-08-08T12:05:00.545852+00:00\",\"dataItems\":[{\"id\":\"grid_power\",\"type\":2,\"label\":\"Netz (kW)\",\"value\":4.77,\"isSensitive\":false},{\"id\":\"house_power\",\"type\":2,\"label\":\"Haus (kW)\",\"value\":0.81,\"isSensitive\":false},{\"id\":\"solar_power\",\"type\":2,\"label\":\"PV (kW)\",\"value\":5.58,\"isSensitive\":false},{\"id\":\"solar_performance\",\"type\":2,\"label\":\"Performance (%)\",\"value\":57,\"isSensitive\":false}]},{\"id\":\"55d0d4ae-c999-4ddc-9054-581636c51836\",\"sourceid\":\"solar\",\"timeStampUtc\":\"2020-08-08T11:50:01.7219556+00:00\",\"dataItems\":[{\"id\":\"grid_power\",\"type\":2,\"label\":\"Netz (kW)\",\"value\":5.07,\"isSensitive\":false},{\"id\":\"house_power\",\"type\":2,\"label\":\"Haus (kW)\",\"value\":0.56,\"isSensitive\":false},{\"id\":\"solar_power\",\"type\":2,\"label\":\"PV (kW)\",\"value\":5.63,\"isSensitive\":false},{\"id\":\"solar_performance\",\"type\":2,\"label\":\"Performance (%)\",\"value\":57,\"isSensitive\":false}]},{\"id\":\"22c4d835-d61f-44dc-b61a-96f2ac16ee7e\",\"sourceid\":\"solar\",\"timeStampUtc\":\"2020-08-08T11:40:01.4672575+00:00\",\"dataItems\":[{\"id\":\"grid_power\",\"type\":2,\"label\":\"Netz (kW)\",\"value\":5.02,\"isSensitive\":false},{\"id\":\"house_power\",\"type\":2,\"label\":\"Haus (kW)\",\"value\":0.66,\"isSensitive\":false},{\"id\":\"solar_power\",\"type\":2,\"label\":\"PV (kW)\",\"value\":5.68,\"isSensitive\":false},{\"id\":\"solar_performance\",\"type\":2,\"label\":\"Performance (%)\",\"value\":58,\"isSensitive\":false}]},{\"id\":\"7447401f-756e-46a9-b4b3-d1e0fd91b275\",\"sourceid\":\"solar\",\"timeStampUtc\":\"2020-08-08T11:35:00.5431542+00:00\",\"dataItems\":[{\"id\":\"grid_power\",\"type\":2,\"label\":\"Netz (kW)\",\"value\":4.93,\"isSensitive\":false},{\"id\":\"house_power\",\"type\":2,\"label\":\"Haus (kW)\",\"value\":0.76,\"isSensitive\":false},{\"id\":\"solar_power\",\"type\":2,\"label\":\"PV (kW)\",\"value\":5.69,\"isSensitive\":false},{\"id\":\"solar_performance\",\"type\":2,\"label\":\"Performance (%)\",\"value\":58,\"isSensitive\":false}]}]}",
			["pellets"] = "{\"sourceId\":\"pellets\",\"historyData\":[{\"id\":\"0039fbcd-1a41-4ae3-b682-0c160e7defda\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-08-08T04:00:08.916429+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"86ffcb00-1997-4e54-8118-570c355c4042\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-08-07T04:00:12.9606362+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"0878ce5c-af5e-40ca-8519-7038047c07a8\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-08-06T04:00:08.6869507+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"34d931bc-4b42-480e-ad68-a35f384bc627\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-08-04T04:00:10.7089831+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"ad11c3aa-2f16-4060-a7ec-fb5d206ea2a9\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-08-02T04:00:09.6992733+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"b213f85a-b3b3-4532-bfc7-d7e27f0b6e47\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-30T04:00:08.0488341+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"d2294858-4728-464d-b78c-a43b797497b9\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-29T14:05:05.057959+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"960f2225-079c-4cdc-afcc-23ba6ba580d3\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-29T14:04:55.435063+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"ff0b2dbf-aa46-47bb-a88d-e013dfad0e95\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-29T13:20:10.012876+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"1030a0c8-6c78-45e3-abe1-7a30579fb3c1\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-29T13:17:43.405318+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"b7979925-4706-4d14-865f-27773cb54f28\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-29T13:03:48.8945153+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"022a03ea-846a-4d7b-9c6c-ef7bb41a4bf9\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-25T04:00:12.5642708+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"9a205af2-d83c-4d45-b105-3e131bcaf66a\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-24T04:00:08.1189355+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"67045afd-18e8-49ca-9f69-6439e086ae8d\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-23T19:55:00.5489872+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"d0441eb6-3c67-4fbf-a289-fbc721807342\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-23T19:28:56.202774+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":4170,\"isSensitive\":false}]},{\"id\":\"d0441eb6-3c67-4fbf-a289-fbc721807343\",\"sourceid\":\"pellets\",\"timeStampUtc\":\"2020-07-22T19:28:56.202774+00:00\",\"dataItems\":[{\"id\":\"supplies\",\"type\":1,\"label\":\"Lager (kg)\",\"value\":0,\"isSensitive\":false}]}]}",
			["tesla"] = "{\"sourceId\":\"tesla\",\"historyData\":[{\"id\":\"b1cb8d4c-2d44-4a29-add6-1e69c4af7a90\",\"sourceid\":\"tesla\",\"timeStampUtc\":\"2020-08-08T10:00:03.3418527+00:00\",\"dataItems\":[{\"id\":\"vehicle_state\",\"type\":2,\"label\":\"Fahrzeugzustand\",\"value\":\"online\",\"isSensitive\":false},{\"id\":\"inside_temperature\",\"type\":2,\"label\":\"Temperatur Innenraum (Grad Celsius)\",\"value\":23.6,\"isSensitive\":false},{\"id\":\"climate_enabled\",\"type\":3,\"label\":\"Heizung/Klimaanlage an\",\"value\":false,\"isSensitive\":false},{\"id\":\"charge_level_percent\",\"type\":1,\"label\":\"Ladezustand (%)\",\"value\":80,\"isSensitive\":false},{\"id\":\"charging_state\",\"type\":1,\"label\":\"Ladevorgang\",\"value\":\"Complete\",\"isSensitive\":false},{\"id\":\"charge_power\",\"type\":2,\"label\":\"Ladeleistung (kW)\",\"value\":0,\"isSensitive\":false}]}]}",
			["wod"] = "{\"sourceId\":\"wod\",\"historyData\":[{\"id\":\"62073a00-0fb0-4689-bdf2-d21b09066fd1\",\"sourceid\":\"wod\",\"timeStampUtc\":\"2020-08-08T04:00:08.51327+00:00\",\"dataItems\":[{\"id\":\"wod_data\",\"type\":1,\"label\":\"WOD\",\"value\":\"<div class=\\\"btwb_webwidget\\\" data-type=\\\"wods\\\" data-sections=\\\"main\\\" data-track_ids=\\\"159835, 305963\\\" data-activity_length=\\\"0\\\" data-leaderboard_length=\\\"0\\\" data-days=\\\"0\\\"><div data-reactroot=\\\"\\\" class=\\\"myContainer\\\"><ul class=\\\"btwb-wod-list\\\"><span><li><h5></h5><small><u><!-- react-text: 10 -->MAIN<!-- /react-text --><!-- react-text: 11 -->:<!-- /react-text --></u><!-- react-text: 12 --> <!-- /react-text --><!-- react-text: 13 -->CrossFit Rosenheim<!-- /react-text --><!-- react-text: 14 --> - <!-- /react-text --><!-- react-text: 15 -->Aug 08, 2020<!-- /react-text --></small><div><h6>Team WOD: Clean &amp; Jerks, Partner Med Ball Runs, Rope Climbs, Partner Alternating Wall Balls</h6><p class=\\\"btwb-workout-description\\\">As a Team of 2 complete for time:\\r\\n30 Clean &amp; Jerks (135#/95#)\\r\\n800m Partner Med Ball Run (20#/14#)\\r\\n8 Rope Climbs (15 ft.)\\r\\n20 Clean &amp; Jerks \\r\\n600m Partner Med Ball Run \\r\\n6 Rope Climbs \\r\\n10 Clean &amp; Jerks \\r\\n400m Partner Med Ball Run \\r\\n4 Rope Climbs \\r\\n50 Partner Alternating Wall Balls (20#/14#)\\r\\n\\r\\n* One bar, one ball. Alternate work with your partner on Clean and Jerks and Rope Climbs. \\r\\n** Run together alternating who carries the med ball. Alternate wall ball throws with partner for 50 total reps between partners (25 each)\\r\\n*** Scale 1 Rope Climb = 2 Supine Rope Climbs\\r\\n\\r\\nPost total time.</p></div><p><i></i></p><hr><div class=\\\"btwb-leaderboard\\\"><div class=\\\"align\\\"></div></div></li></span></ul><ul class=\\\"btwb-wod-list\\\"><span><li><h5></h5><small><u><!-- react-text: 30 -->MAIN<!-- /react-text --><!-- react-text: 31 -->:<!-- /react-text --></u><!-- react-text: 32 --> <!-- /react-text --><!-- react-text: 33 -->Easy Class<!-- /react-text --><!-- react-text: 34 --> - <!-- /react-text --><!-- react-text: 35 -->Aug 08, 2020<!-- /react-text --></small><div><h6>Jack</h6><p class=\\\"btwb-workout-description\\\">Complete as many rounds as possible in 20 mins of:\\r\\n10 Push Press, 115/85 lbs\\r\\n10 Kettlebell Swings, 1.5/1 pood\\r\\n10 Box Jumps, 24/20 in\\r\\n\\r\\nArmy Staff Sgt. Jack M. Martin III, 26, of Bethany, Oklahoma, assigned to the 3rd Battalion, 1st Special Forces Group, Fort Lewis, Wash., died September 29th, 2009, in Jolo Island, Philippines, from the detonation of an improvised explosive device. Martin is survived by his wife Ashley Martin, his parents Jack and Cheryl Martin, and siblings Abe, Mandi, Amber and Abi.</p></div><p><i>Locker, leichtes  Gewicht etc. Wenn möglich Springen statt Steigen. Scale Höhe etc.</i></p><hr><div class=\\\"btwb-leaderboard\\\"><div class=\\\"align\\\"></div></div></li></span></ul></div></div>\",\"isSensitive\":false}]}]}"
		};

		protected bool _useMockData = false;

		protected async Task<SourceHistory> GetSourceHistory(string sourceId, int dataPoints = 1)
		{
			var authKey = Configuration["FunctionsAuthKey"];
			var baseUrl = Configuration["ApiBaseUrl"];

			if(_useMockData)
			{
				//await Task.Delay(2000);
				return JsonConvert.DeserializeObject<SourceHistory>(_mockData[sourceId]);
			}

			var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/sourcedata/{sourceId}?numDataPoints={dataPoints}");
			request.Headers.Add("x-functions-key", authKey);

			try
			{
				var response = await HttpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();
				var historyData = JsonConvert.DeserializeObject<SourceHistory>(responseContent);

				//Console.WriteLine(responseContent);
				return historyData;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to get source data: " + ex);
				throw;
			}
		}

		public async Task UpdateInitialHistory()
		{
			InitialHistory = await GetSourceHistory(SourceId, NumInitialDataPoints);
			TimeStampSource = InitialHistory.HistoryData.FirstOrDefault()?.TimeStampUtc.ToLocalTime().ToString("g");
		}

		protected virtual Task InitialHistoryLoaded() { return Task.CompletedTask; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if(!firstRender)
			{
				return;
			}

			await UpdateInitialHistory();
			await InitialHistoryLoaded();
			StateHasChanged();
		}
	}
}