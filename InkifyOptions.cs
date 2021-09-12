using System;
using System.IO;
using System.Linq;

using cmdwtf.Dithering.ColorReduction;

namespace Inkify
{
	internal class InkifyOptions
	{
		public bool Verbose { get; set; } = false;
		public InkyFlags Flags { get; set; } = (InkyFlags)InkyDevice.Impression;
		public InkyDevice Device
		{
			get => (InkyDevice)Flags;
			set => Flags = (InkyFlags)value;
		}
		public FileInfo[] Input { get; set; } = Array.Empty<FileInfo>();
		public FileInfo? Output { get; set; } = null;
		public IErrorDiffusion[] Ditherers { get; private set; } = new[] { NopDithering.Instance };
		public string Ditherer
		{
			set => Ditherers = value.ToLowerInvariant() == "all"
				? DithererChoices.Ditherers.Values.ToArray()
				: new[] { DithererChoices.Ditherers[value] };
			get => Ditherers.Length > 1 ? "All" : Ditherers.First().Name;
		}
		public float Saturation { get; set; } = 0;
	}
}