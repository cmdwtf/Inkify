using System.IO;

using cmdwtf.Dithering.ColorReduction;
using cmdwtf.Dithering.Transforms;

namespace Inkify
{
	internal record ImageTransformJob(FileInfo Input, InkyDevice TargetDevice, IErrorDiffusion Ditherer)
	{
		public IPixelTransform? CustomTransform { get; init; }
	}
}
