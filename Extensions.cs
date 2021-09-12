using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

using cmdwtf.Dithering;
using cmdwtf.Dithering.Extensions;

namespace Inkify
{
	public static class Extensions
	{
		private static Dictionary<ArgbColor, byte> BuildPaletteLookup(this IEnumerable<ArgbColor> palette)
		{
			Dictionary<ArgbColor, byte> result = new();
			int paletteSize = palette.Count();

			for (byte scan = 0; scan < paletteSize; ++scan)
			{
				ArgbColor key = palette.ElementAt(scan);
				if (result.ContainsKey(key))
				{
					continue;
				}
				result.Add(key, scan);
			}

			return result;
		}

		private static Color ToSystemDrawingColor(this ArgbColor color) => Color.FromArgb(color.ToArgb());

		public static async Task<Bitmap?> To8bppIndexedFormatAsync(this Bitmap source, IEnumerable<ArgbColor> palette, ArgbColor[] pixels)
		{
			int imageColorCount = pixels.GetColorCount();
			ArgbColor[] paletteArray = palette as ArgbColor[] ?? palette.ToArray();
			int paletteLength = paletteArray.Length;

			if (imageColorCount > paletteLength)
			{
				throw new ArgumentOutOfRangeException(nameof(pixels),
					"Image pixel array uses more colors than are available in the palette.");
			}

			// System.Windows.Media seems to have a FormatConvertedBitmap()
			// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-convert-a-bitmapsource-to-an-indexed-pixel-format?view=netframeworkdesktop-4.8
			// but since we are trying to stay platform agnostic, we'll do it the hard way.

			PixelFormat format = PixelFormat.Format8bppIndexed;

			Dictionary<ArgbColor, byte> lookup = paletteArray.BuildPaletteLookup();

			var clone = new Bitmap(source.Width, source.Height, format);

			// apparently this is how you have to modify the palette...
			ColorPalette clonePalette = clone.Palette;
			int newPaletteLength = Math.Min(clone.Palette.Entries.Length, paletteLength);

			for (int scan = 0; scan < newPaletteLength; ++scan)
			{
				clonePalette.Entries[scan] = paletteArray[scan].ToSystemDrawingColor();
			}

			clone.Palette = clonePalette;

			var task = Task.Run(() =>
			{
				// can't use drawing on an indexed bitmap...
				//using var graphics = Graphics.FromImage(clone);
				//graphics.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));

				Rectangle rect = new(0, 0, source.Width, source.Height);
				BitmapData cloneData = clone.LockBits(rect, ImageLockMode.WriteOnly, format);

				try
				{
					for (int yScan = 0; yScan < source.Height; ++yScan)
					{
						unsafe
						{
							byte* cloneRow = (byte*)(cloneData.Scan0 + (cloneData.Stride * yScan));
							for (int xScan = 0; xScan < source.Width; ++xScan)
							{
								int offset = source.Width * yScan;
								cloneRow[xScan] = lookup[pixels[offset + xScan]];
							}
						}
					}
				}
				catch (KeyNotFoundException knfex)
				{
					throw new IndexOutOfRangeException(
						$"Unable to create indexed image. It's likely that the image uses colors not in the palette. {knfex.Message}");
				}

				return clone;
			});

			return await task.ConfigureAwait(false);
		}
	}
}
