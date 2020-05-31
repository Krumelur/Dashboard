using System.Diagnostics;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace Dashboard.Server.Sources.SolarEdge
{
	public class SolarEdgeHelper
	{
		readonly static string SOLAR_EDGE_BASE_URL = "https://monitoringapi.solaredge.com/site";

		public SolarEdgeHelper(int installationId, string apiKey, int timeoutSeconds, ILogger logger)
		{
			_installationId = installationId;
			_apiKey = apiKey;
			_timeoutSeconds = timeoutSeconds;
			_logger = logger;

			FlurlHttp.Configure(settings => settings.OnError = LogHttpError);
		}

		readonly int _timeoutSeconds;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly string _apiKey;
		readonly int _installationId;
		readonly ILogger _logger;

		public async Task<CurrentPowerFlow> GetCurrentPowerAsync()
		{
			var result = await SOLAR_EDGE_BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegment(_installationId)
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
