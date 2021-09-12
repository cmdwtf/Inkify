using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using cmdwtf.Dithering.ColorReduction;
using cmdwtf.Dithering.Transforms;

using Microsoft.Extensions.Logging;

namespace Inkify
{
	internal class Inkify
	{
		private record TransformResult(FileInfo Input, IErrorDiffusion Ditherer, Bitmap Bitmap);
		private ImageTransformer Transformer { get; init; }
		private ILogger Logger { get; init; }

		public Inkify(ImageTransformer transformer, ILogger<Inkify> logger)
		{
			Transformer = transformer;
			Logger = logger;
		}

		public async Task<int> RunTransformation(InkifyOptions options)
		{
			int targetTransformationCount = options.Input.Length * options.Ditherers.Length;
			int successfulTransforms = 0;
			bool generateOutputNames = false;

			if (targetTransformationCount > 1 && options.Output is not null)
			{
				Logger.LogCritical("More than one input file specified with an output option also specified." + Environment.NewLine +
								   "No output option may be specified for converting multiple input files.");
				return -1;
			}

			if (targetTransformationCount > 1 || options.Output is null)
			{
				generateOutputNames = true;
			}

			await foreach (TransformResult result in GetTransformedImages(options))
			{
				FileInfo output;

				if (generateOutputNames || options.Output is null)
				{
					string outputDirectory =
						result.Input.DirectoryName ?? throw new InvalidOperationException("DirectoryName was null.");
					string outputBaseName = Path.GetFileNameWithoutExtension(result.Input.Name);
					string saturation = options.Saturation == 0 ? string.Empty : $"{options.Saturation}";
					string runName = $"{options.Device}{saturation}.{result.Ditherer.Name}";
					string outputPath = Path.Combine(outputDirectory, $"{outputBaseName}.{runName}.png");
					output = new FileInfo(outputPath);
				}
				else
				{
					output = options.Output;
				}

				if (SavePng(result.Bitmap, output))
				{
					successfulTransforms++;
					Logger.LogInformation("Wrote image to `{output}`", output.FullName);
				}

				result.Bitmap.Dispose();
			}

			return targetTransformationCount - successfulTransforms;
		}

		private async IAsyncEnumerable<TransformResult> GetTransformedImages(InkifyOptions options)
		{
			foreach (FileInfo input in options.Input)
			{
				foreach (IErrorDiffusion ditherer in options.Ditherers)
				{
					var job = new ImageTransformJob(input, options.Device, ditherer)
					{
						CustomTransform = options.Saturation != 0
							? SimpleIndexedPalettePixelTransformInky.InkyImpression7Blended(options.Saturation)
							: null
					};

					Bitmap? transformedBitmap = await Transformer.TransformAsync(job);

					if (transformedBitmap is not null)
					{
						yield return new TransformResult(input, ditherer, transformedBitmap);
					}
				}
			}
		}

		private bool SavePng(Bitmap bitmap, FileInfo file)
		{
			try
			{
				ImageCodecInfo? pngEncoder = GetEncoder(ImageFormat.Png);
				var encoderParams = new EncoderParameters()
				{
					Param = new EncoderParameter[]
					{
						new(Encoder.ColorDepth, 8),
					}
				};

				bitmap.Save(file.FullName, pngEncoder, encoderParams);
				return true;
			}
			catch (Exception ex)
			{
				Logger.LogCritical("Failed to save png: {msg}", ex.Message);
				return false;
			}
		}

		private static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo? ici = ImageCodecInfo
				.GetImageEncoders()
				.FirstOrDefault(c => c.FormatID == format.Guid);

			return ici ?? throw new NotImplementedException($"Couldn't get encoder for {format}.");
		}
	}
}