using System;
using Microsoft.AspNetCore.Http;

namespace Dashboard.API
{
	public static class Helpers
	{
		public static bool CheckFunctionAuthKey(string requiredKey, HttpRequest req)
		{
			// Never check any keys when running locally.
			if(req.Host.Host == "127.0.0.1" || req.Host.Host.ToLowerInvariant() == "localhost")
			{
				return true;
			}

			if(String.IsNullOrWhiteSpace(requiredKey))
			{
				return false;
			}

			if(req.Headers["x-functions-key"] != requiredKey)
			{
				return false;
			}

			return true;
		}

		public static bool IsSensitiveDataPinProvided(string requiredPin, HttpRequest req)
		{
			// Never check any keys when running locally.
			if(req.Host.Host == "127.0.0.1" || req.Host.Host.ToLowerInvariant() == "localhost")
			{
				return true;
			}

			if(String.IsNullOrWhiteSpace(requiredPin))
			{
				return false;
			}

			if(req.Headers["sensitive-data-pin"] != requiredPin)
			{
				return false;
			}

			return true;
		}
	}
}