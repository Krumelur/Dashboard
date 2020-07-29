using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dashboard.Support.Tesla 
{
	/// <summary>
	/// Allows communication with Tesla's restful APIs.
	/// Possible thanks to: https://tesla-api.timdorr.com
	/// </summary>
	public class TeslaApi
	{
		readonly string BASE_URL = "https://owner-api.teslamotors.com";
		
		LoginResponse _loginResponse;
		readonly string _userAgent;
		readonly int _timeoutSeconds;
		readonly string _teslaClientId;
		readonly string _teslaClientSecret;

		public TeslaApi(string teslaClientId, string teslaClientSecret, string userAgent, int timeoutSeconds)
		{
			FlurlHttp.Configure(settings => settings.OnError = LogFlurlError);
			_userAgent = userAgent;
			_timeoutSeconds = timeoutSeconds;
			_teslaClientId = teslaClientId;
			_teslaClientSecret = teslaClientSecret;
		}

		void LogFlurlError(HttpCall call)
		{
			Debug.WriteLine($"[{nameof(TeslaApi)}] Error: {call.Exception.Message}");
			call.ExceptionHandled = false;
		}

		/// <summary>
		/// Login to Tesla account.
		/// </summary>
		/// <param name="username">username</param>
		/// <param name="password">password</param>
		/// <param name="cancellationToken"></param>
		/// <returns>login result</returns>
		public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default(CancellationToken))
		{
			_loginResponse = await BASE_URL
				.AppendPathSegment("oauth/token")
				.WithTimeout(_timeoutSeconds)
				.WithHeader("User-Agent", _userAgent)
				.PostJsonAsync(new
				{
					grant_type = "password",
					client_id = _teslaClientId,
					client_secret = _teslaClientSecret,
					email = username,
					password = password
				})
				.ReceiveJson<LoginResponse>()
				.ConfigureAwait(false);

			return _loginResponse;
		}

		/// <summary>
		/// Returns if the vehicle allows remote access.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <returns></returns>
		public async Task<bool> AllowsMobileAccessAsync(long id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var response = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id, "mobile_enabled")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.GetStringAsync(cancellationToken)
				.ConfigureAwait(false);

			bool enabled = Convert.ToBoolean(JObject.Parse(response)["response"].ToString());
			return enabled;
		}

		/// <summary>
		/// Refreshes the login using the token received when logging in with username and password.
		/// </summary>
		/// <returns></returns>
		public async Task<LoginResponse> RefreshToken(CancellationToken cancellationToken = default(CancellationToken))
		{
			_loginResponse = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegment("oauth/token")
				.WithHeader("User-Agent", _userAgent)
				.PostJsonAsync(new
				{
					grant_type = "password",
					client_id = _teslaClientId,
					client_secret = _teslaClientSecret,
					refresh_token = _loginResponse.RefreshToken
				}, cancellationToken)
				.ReceiveJson<LoginResponse>()
				.ConfigureAwait(false);

			return _loginResponse;
		}

		/// <summary>
		/// Get list of vehicles.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<List<VehicleResponse>> GetAllVehiclesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var response = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegment("api/1/vehicles")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.GetJsonAsync<VehicleListResponse>(cancellationToken)
				.ConfigureAwait(false);

			return response.Vehicles.ToList();
		}

		/// <summary>
		/// Get a sepcific vehicles.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<VehicleResponse> GetVehicleAsync(long id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var response = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id)
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.GetStringAsync(cancellationToken)
				.ConfigureAwait(false);

			var vehicle = ExtractInnerResponse<VehicleResponse>(response);

			return vehicle;
		}

		/// <summary>
		/// Wakes the vehicle up. If a vehicle is sleeping, all calls will immediately timeout and return HTTP status 408.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<VehicleResponse> WakeUpVehicleAsync(long id, int timeoutSeconds, CancellationToken cancellationToken = default(CancellationToken))
		{
			var result = await BASE_URL
				.WithTimeout(timeoutSeconds == 0 ? _timeoutSeconds : timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id, "wake_up")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.PostStringAsync(string.Empty, cancellationToken)
				.ReceiveString()
				.ConfigureAwait(false);

			var vehicleStatus = ExtractInnerResponse<VehicleResponse>(result);
			return vehicleStatus;
		}

		/// <summary>
		/// Gets data about a sepcific vehicle. The returned data is a combination of smaller pieces of information that can also be returned separately.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<VehicleDataResponse> GetVehicleDataAsync(long id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var response = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id, "/vehicle_data")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.GetStringAsync(cancellationToken)
				.ConfigureAwait(false);

			var vehicleData= ExtractInnerResponse<VehicleDataResponse>(response);

			return vehicleData;
		}

		/// <summary>
		/// Gets status information about a sepcific vehicle.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<VehicleStateResponse> GetVehicleStateAsync(long id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var response = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id, "data_request/vehicle_state")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.GetStringAsync(cancellationToken)
				.ConfigureAwait(false);

			var vehicleState = ExtractInnerResponse<VehicleStateResponse>(response);

			return vehicleState;
		}

		/// <summary>
		/// Gets drive information about a sepcific vehicle.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<DriveStateResponse> GetDriveStateAsync(long id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var response = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id, "data_request/drive_state")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.GetStringAsync(cancellationToken)
				.ConfigureAwait(false);

			var driveState = ExtractInnerResponse<DriveStateResponse>(response);

			return driveState;
		}

		/// <summary>
		/// Enables or disables climate control
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="enabled"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<StatusResponse> EnableClimateControlAsync(long id, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
		{
			var result = await BASE_URL
				.WithTimeout(_timeoutSeconds)
				.AppendPathSegments("api/1/vehicles", id, "command", enabled ? "auto_conditioning_start" : "auto_conditioning_stop")
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.PostStringAsync(string.Empty, cancellationToken)
				.ReceiveString()
				.ConfigureAwait(false);

			var status = ExtractInnerResponse<StatusResponse>(result);
			return status;
		}

		/// <summary>
		/// Locks or unlocks the doors.
		/// </summary>
		/// <param name="id">ID of the vehicle. This is the Id propety of VehicleResponse and NOT the VehicleId property!</param>
		/// <param name="enabled"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<StatusResponse> LockDoorsAsync(long id, bool locked, CancellationToken cancellationToken = default(CancellationToken))
		{
			var result = await BASE_URL
				.AppendPathSegments("api/1/vehicles", id, "command", locked ? "door_lock" : "door_unlock")
				.WithTimeout(_timeoutSeconds)
				.WithHeader("User-Agent", _userAgent)
				.WithOAuthBearerToken(_loginResponse.AccessToken)
				.PostStringAsync(string.Empty, cancellationToken)
				.ReceiveString()
				.ConfigureAwait(false);

			var status = ExtractInnerResponse<StatusResponse>(result);
			return status;
		}

		/// <summary>
		/// Helper method to extract nested data from Tesla's API response.
		/// They like to return data inside a parent object with the key "response".
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		T ExtractInnerResponse<T>(string json) where T : class
		{
			var jObject = JObject.Parse(json);
			var innerObject = JsonConvert.DeserializeObject<T>(jObject["response"].ToString());
			return innerObject;
		}
	}
}
