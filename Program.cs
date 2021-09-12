using System;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inkify
{
	internal static class Program
	{
		private static async Task<int> Main(string[] args)
		{
			SetWindowTitle();
			return await HandleCommandLine(args);
		}

		private static async Task<int> HandleCommandLine(string[] args)
		{
			int result = await InkifyCommand.BuildCommandLine()
				.UseDefaults()
				.UseHost(GetHostBuilder, ConfigureHost)
				.Build()
				.InvokeAsync(args);
			return result;
		}

		private static void ConfigureHost(IHostBuilder host)
		{
			host.ConfigureServices(services =>
			{
				// register services
				services.AddSingleton<Inkify>();
				services.AddSingleton<ImageTransformer>();
			});
		}

		private static IHostBuilder GetHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logging =>
				{
					// add our custom minimum level filter.
					logging.AddFilter(MinimumLogLevelFilter.Filter);
				});
		}

		private static void SetWindowTitle()
		{
			var asy = Assembly.GetExecutingAssembly();
			if (string.IsNullOrWhiteSpace(asy.FullName) == false)
			{
				Console.Title = asy.FullName;
			}
		}
	}
}