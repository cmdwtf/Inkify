using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using cmdwtf.Dithering;
using cmdwtf.Dithering.ColorReduction;
using cmdwtf.Dithering.Extensions;
using cmdwtf.Dithering.Transforms;

using Microsoft.Extensions.Logging;

using static cmdwtf.Dithering.Transforms.SimpleIndexedPalettePixelTransformInky;

namespace Inkify
{
	internal class ImageTransformer
	{
		private static readonly Dictionary<InkyDevice, InkifyTarget> _targets = new()
		{
			{ InkyDevice.Impression, new InkifyTarget { Width = 600, Height = 448, Transform = InkyImpression7Saturated } },
			{ InkyDevice.ImpressionDesaturated, new InkifyTarget { Width = 600, Height = 448, Transform = InkyImpression7 } },
			{ InkyDevice.WhatRed, new InkifyTarget { Width = 400, Height = 300, Transform = Inky3Red } },
			{ InkyDevice.WhatYellow, new InkifyTarget { Width = 400, Height = 300, Transform = Inky3Yellow } },
			{ InkyDevice.WhatMono, new InkifyTarget { Width = 400, Height = 300, Transform = Inky2 } },
			{ InkyDevice.PhatRed, new InkifyTarget { Width = 250, Height = 122, Transform = Inky3Red } },
			{ InkyDevice.PhatYellow, new InkifyTarget { Width = 250, Height = 122, Transform = Inky3Yellow } },
			{ InkyDevice.PhatMono, new InkifyTarget { Width = 250, Height = 122, Transform = Inky2 } },
		};

		private ILogger Logger { get; init; }

		public ImageTransformer(ILogger<ImageTransformer> logger)
		{
			Logger = logger;
		}

		public static InkifyTarget GetTarget(InkyDevice targetFlags)
		{
			if (_targets.ContainsKey(targetFlags))
			{
				return _targets[targetFlags];
			}

			throw new KeyNotFoundException($"Unable to find a target matching flags: {targetFlags}");
		}

		public async Task<Bitmap?> TransformAsync(ImageTransformJob job)
		{
			Logger.LogDebug("Starting transformation.");

			InkifyTarget target = GetTarget(job.TargetDevice);
			IPixelTransform transform = job.CustomTransform ?? target.Transform;
			Bitmap scaled;

			try
			{
				scaled = await LoadAndScaleAsync(job.Input, target);
			}
			catch (Exception ex)
			{
				Logger.LogCritical("Failed to load and scale `{fileName}`: {ex}", job.Input.FullName, ex.Message);
				return null;
			}

			Logger.LogInformation("Transforming: `{input.Name}`", job.Input.Name);

			string runName = $"{transform.Name}-{job.Ditherer.Name}";

			try
			{
				Logger.LogDebug("Transform run: {runName}", runName);
				return await TransformImageAsync(scaled, transform, job.Ditherer);
			}
			catch (Exception)
			{
				Logger.LogCritical("Failed to transform for run {runName}", runName);
			}

			return null;
		}

		private async Task<Bitmap?> TransformImageAsync(Bitmap scaled, IPixelTransform transform, IErrorDiffusion dither)
		{
			Bitmap transformed;

			try
			{
				transformed = scaled.TransformImage(new TransformationOptions
				{
					Transform = transform,
					Dither = dither
				});
			}
			catch (Exception ex)
			{
				Logger.LogCritical("Failed to transform image: {ex}", ex.Message);
				return null;
			}

			try
			{
				ArgbColor[] transformedPixels = transformed.GetPixelsFrom32BitArgbImage();
				Bitmap? indexed = await transformed.To8bppIndexedFormatAsync(transform.Palette, transformedPixels);

				if (indexed is not null)
				{
					return indexed;
				}

				Logger.LogError("Failed to create indexed format bitmap, will write default format.");
				return transformed;
			}
			catch (Exception ex)
			{
				Logger.LogError("Failed to create indexed format bitmap, will write in default format. {ex}", ex.Message);
				return transformed;
			}

		}

		private async Task<Bitmap> LoadAndScaleAsync(FileInfo input, InkifyTarget target)
		{
			if (input.Exists == false)
			{
				Logger.LogCritical("Couldn't find input file: `{file}`, skipping.", input.FullName);
				throw new FileNotFoundException($"Input file doesn't exist: `{input.FullName}`");
			}

			var task = Task.Run(() =>
			{
				Logger.LogInformation("Loading `{input.FullName}`", input.FullName);
				using var original = Image.FromFile(input.FullName);

				Logger.LogDebug("Scaling `{input.FullName}`", input.FullName);

				return original.FitToResolution(target.Width, target.Height) as Bitmap;
			});

			Bitmap? result = await task.ConfigureAwait(false);

			if (result is not null)
			{
				return result;
			}

			Logger.LogCritical("Failed to scale `{file}`", input.FullName);
			throw new InvalidOperationException($"Failed to scale `{input.FullName}`");
		}

	}
}