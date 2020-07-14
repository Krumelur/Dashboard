using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);

			builder.Services
			.AddBlazorise(options => {
				 options.ChangeTextOnKeyPress = true;
				})
			.AddBootstrapProviders()
			.AddFontAwesomeIcons()
			.AddSingleton(new HttpClient {
				BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
			})
			.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

			builder.RootComponents.Add<App>("app");

			var host = builder.Build();

			host.Services
			  .UseBootstrapProviders()
			  .UseFontAwesomeIcons();

			await host.RunAsync();
		}
	}
}
