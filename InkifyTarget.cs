using cmdwtf.Dithering.Transforms;

namespace Inkify
{
	internal record InkifyTarget()
	{
		public int Width { get; init; }
		public int Height { get; init; }
		public IPixelTransform Transform { get; init; } = NopPixelTransform.Instance;
		public static InkifyTarget None { get; } = new();
	}
}
