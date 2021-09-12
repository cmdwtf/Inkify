using System;

namespace Inkify
{
	[Flags]
	internal enum InkyFlags
	{
		None = 0x00,

		ColorBlackWhite = 0x01,
		ColorBlackWhiteRed = 0x02,
		ColorBlackWhiteYellow = 0x04,
		Color7Color = 0x08,
		Color7ColorDesaturated = 0x10,

		SizePhat = 0x20,
		SizeWhat = 0x40,
		SizeImpression = 0x80,
	}

	[Flags]
	internal enum InkyDevice
	{
		Impression = InkyFlags.SizeImpression | InkyFlags.Color7Color,
		ImpressionDesaturated = InkyFlags.SizeImpression | InkyFlags.Color7ColorDesaturated,
		WhatRed = InkyFlags.SizeWhat | InkyFlags.ColorBlackWhiteRed,
		WhatYellow = InkyFlags.SizeWhat | InkyFlags.ColorBlackWhiteYellow,
		WhatMono = InkyFlags.SizeWhat | InkyFlags.ColorBlackWhite,
		PhatRed = InkyFlags.SizePhat | InkyFlags.ColorBlackWhiteRed,
		PhatYellow = InkyFlags.SizePhat | InkyFlags.ColorBlackWhiteYellow,
		PhatMono = InkyFlags.SizePhat | InkyFlags.ColorBlackWhite,
	}
}