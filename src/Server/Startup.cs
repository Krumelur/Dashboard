using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Dashboard.Server.Startup))]

namespace Dashboard.Server
{
	public class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			var defaultConfig = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

			// IConfiguration provided by Functions SDK does not contain user secrets. 
			// Add them manually.
			// See also: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#function-app-provided-services
			var config = new ConfigurationBuilder()
				.AddConfiguration(defaultConfig)
				.AddUserSecrets<Startup>()
				.Build();

			builder.Services.AddSingleton<IConfiguration>(config);
		}
	}
}
