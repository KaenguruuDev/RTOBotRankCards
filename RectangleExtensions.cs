using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace RTOSharpRankCards;

public static class RectangleExtensions
{
	public static void FillRoundedRectangle(this IImageProcessingContext ctx, SixLabors.ImageSharp.Color color,
		RectangleF rect, float radius)
	{
		// Build the path for a rounded rectangle
		var x0 = rect.Left;
		var y0 = rect.Top;
		var x1 = rect.Right;
		var y1 = rect.Bottom;

		var path = new PathCollection(
			new RectangularPolygon(x0 + radius, y0, rect.Width - 2 * radius, rect.Height), // center rect
			new RectangularPolygon(x0, y0 + radius, rect.Width, rect.Height - 2 * radius), // middle rect
			new EllipsePolygon(x0 + radius, y0 + radius, radius), // top-left corner
			new EllipsePolygon(x1 - radius, y0 + radius, radius), // top-right corner
			new EllipsePolygon(x0 + radius, y1 - radius, radius), // bottom-left corner
			new EllipsePolygon(x1 - radius, y1 - radius, radius) // bottom-right corner
		);

		ctx.Fill(color, path);
	}
}