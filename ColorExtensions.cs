using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class ColorExtensions
{
	public static Color Darken(this Color color, float percent)
	{
		// percent = 0.2f means darken by 20%
		percent = Math.Clamp(percent, 0f, 1f);

		var rgba = color.ToPixel<Rgba32>();
		var r = (byte)(rgba.R * (1 - percent));
		var g = (byte)(rgba.G * (1 - percent));
		var b = (byte)(rgba.B * (1 - percent));

		return Color.FromRgba(r, g, b, rgba.A);
	}
}