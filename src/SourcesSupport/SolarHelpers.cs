using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace SourcesSupport
{
	public class SolarHelper
	{
		readonly static string SOLAR_EDGE_BASE_URL = "https://monitoringapi.solaredge.com/site";

		public SolarHelper(int installationId, string apiKey, ILogger logger)
		{
			_installationId = installationId;
			_apiKey = apiKey;
			_logger = logger;

			FlurlHttp.Configure(settings => settings.OnError = LogHttpError);
			FlurlHttp.GlobalSettings.Timeout = TimeSpan.FromSeconds(20);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly string _apiKey;
		readonly int _installationId;
		ILogger _logger;

		public async Task<CurrentPowerFlow> GetCurrentPowerAsync()
		{
			var result = await SOLAR_EDGE_BASE_URL
				.AppendPathSegment("currentPowerFlow")
				.SetQueryParam("api_key", _apiKey)
				.GetJsonAsync<CurrentPowerFlowRoot>()
				.ConfigureAwait(false);

			return result.CurrentPowerFlow;
		}

		void LogHttpError(HttpCall httpCall)
		{
			if (_logger != null)
			{
				_logger.LogError($"HTTP error: {httpCall.Exception}");
			}
		}
	}
}
