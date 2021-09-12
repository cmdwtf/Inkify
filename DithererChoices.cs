using System.Collections.Generic;

using cmdwtf.Dithering.ColorReduction;

namespace Inkify
{
	internal static class DithererChoices
	{
		public static Dictionary<string, IErrorDiffusion> Ditherers { get; private set; } = new();

		static DithererChoices()
		{
			IErrorDiffusion[] ditherers = {
				new StuckiDithering(),
				new FloydSteinbergDithering(),
				new SierraLiteDithering(),
				new Sierra3Dithering(),
				new Sierra2Dithering(),
				new JarvisJudiceNinkeDithering(),
				new AtkinsonDithering(),
				new BurksDithering(),
#if ENABLE_UNBALANCED_DITHERERS
				new Bayer2(),
				new Bayer3(),
				new Bayer4(),
				new Bayer8(),
				new RandomDithering(),
#endif // ENABLE_UNBALANCED_DITHERERS
			};

			foreach (IErrorDiffusion ditherer in ditherers)
			{
				Ditherers.Add(ditherer.Name, ditherer);
			}
		}
	}
}