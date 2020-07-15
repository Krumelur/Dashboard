using Newtonsoft.Json;
using System;

namespace Dashboard.Server.DataSources.Tesla.Responses
{
	/// <summary>
	/// Response of login call.
	/// </summary>
	public class LoginResponse
	{
		/// <summary>
		/// Access token for API calls.
		/// </summary>
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		/// <summary>
		/// Refresh token to get new access token.
		/// </summary>
		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; }

		/// <summary>
		/// Expiration time of token in seconds.
		/// </summary>
		[JsonProperty("expires_in")]
		public int ExpiresInRaw { get; set; }

		/// <summary>
		/// Expiration time as a TimeSpan.
		/// </summary>
		[JsonIgnore]
		public TimeSpan ExpiresIn => TimeSpan.FromSeconds(ExpiresInRaw);

		public override string ToString() => $"[{nameof(LoginResponse)}] AccessToken = {(string.IsNullOrWhiteSpace(AccessToken) ? "not set" : (AccessToken.Substring(0, 3) + "..."))}; RefreshToken = {(string.IsNullOrWhiteSpace(RefreshToken) ? "not set" : (RefreshToken.Substring(0, 3) + "..."))}; ExpiresIn = {ExpiresInRaw}";
	}
}
