using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using cmdwtf.Dithering.ColorReduction;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inkify
{
	internal class InkifyCommand
	{
		public static CommandLineBuilder BuildCommandLine()
		{
			Command listCommand = new("--ditherers")
			{
				Description = "List supported dithering algorithms.",
				Handler = CommandHandler.Create<IHost, IConsole>(HandleListDevicesAsync)
			};
			listCommand.AddAlias("ditherers");

			Command transformCommand = new("--transform")
			{
				Description = "Transforms an image to be suitable for display on an eInk device.",
				Handler = CommandHandler.Create<IHost, InkifyOptions>(HandleTransformAsync),
				IsHidden = false,
			};
			AddTransformCommandOptions(transformCommand);

			RootCommand root = new()
			{
				transformCommand,
				listCommand,
			};
			root.Name = nameof(Inkify).ToLower();

			AddGlobalOptions(root);

			// make the transform command the default subcommand if one isn't specified.
			//root.Handler = transformCommand.Handler;

			return new CommandLineBuilder(root);
		}

		private static void AddTransformCommandOptions(Command cmd)
		{
			cmd.AddArgument(new Argument<FileInfo[]>("input")
			{
				Description = "Image file(s) to inkify.",
				Arity = ArgumentArity.OneOrMore,
			}.ExistingOnly());

			cmd.AddOption(new Option<string>(new[] { "--output", "-o" })
			{
				Description = "Output filename.",
				Arity = ArgumentArity.ZeroOrOne,
				IsRequired = false,
			});

			cmd.AddOption(new Option<float>(new[] { "--saturation", "-s" })
			{
				Description = "The saturation blend level for the Inky 7 color palette.",
				Arity = ArgumentArity.ExactlyOne,
				IsRequired = false,
			});

			Option<InkyDevice> deviceOption = new(new[] { "--device", "-d" })
			{
				Description = "Target Inky display.",
				Arity = ArgumentArity.ExactlyOne,
				IsRequired = false,
			};
			deviceOption.SetDefaultValue(InkyDevice.Impression);
			cmd.AddOption(deviceOption);

			var ditherOption = new Option<string>(new[] { "--ditherer", "-e" })
			{
				Description = "Dithering algorithm to use.",
				Arity = ArgumentArity.ExactlyOne,
				IsRequired = false,
			};
			ditherOption.SetDefaultValue(DithererChoices.Ditherers.FirstOrDefault().Key);
			ditherOption.AddValidator(ValidateDitherer);
			cmd.AddOption(ditherOption);

			cmd.AddAlias("transform");
		}

		private static void AddGlobalOptions(RootCommand root)
		{
			Option<bool> verbose = new(new[] { "--verbose", "-v" })
			{
				Description = "Show debug output",
				IsRequired = false
			};
			verbose.SetDefaultValue(false);

			root.AddGlobalOption(verbose);
		}

		private static string? ValidateDitherer(OptionResult symbolResult)
		{
			if (symbolResult.IsImplicit)
			{
				return null;
			}

			string choice;

			try
			{
				choice = symbolResult.GetValueOrDefault<string>()
						 ?? throw new NullReferenceException("Symbol result must have a value if not implicit.");
			}
			catch (InvalidOperationException)
			{
				return null;
			}

			// all is a peachy option.
			if (choice.ToLowerInvariant() == "all")
			{
				return null;
			}

			return DithererChoices.Ditherers.ContainsKey(choice)
				? null
				: $"`{choice}` not a valid ditherer. Use the `ditherers` subcommand to see a list of valid options.";
		}

		private static async Task<int> HandleTransformAsync(IHost host, InkifyOptions options)
		{
			// enable the default log level
			MinimumLogLevelFilter.Level = LogLevel.Information;

			// or bump it and tell the user if they asked for verbose.
			if (options.Verbose)
			{
				MinimumLogLevelFilter.Level = LogLevel.Debug;

				ILogger<InkifyCommand>? logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<InkifyCommand>();
				logger.LogDebug("Enabled verbose output.");
			}

			Inkify inkify = host.Services.GetRequiredService<Inkify>();
			int result = await inkify.RunTransformation(options);

			// turn logging back off to quiet up host teardown.
			MinimumLogLevelFilter.Level = LogLevel.None;

			return result;
		}

		private static Task<int> HandleListDevicesAsync(IHost host, IConsole console)
		{
			console.Out.WriteLine("Dithering Choices:");
			console.Out.WriteLine("==================");
			console.Out.WriteLine($"> All");

			foreach (KeyValuePair<string, IErrorDiffusion> pair in DithererChoices.Ditherers)
			{
				console.Out.WriteLine($"> {pair.Key}");
			}

			return Task.FromResult(0);
		}
	}
}