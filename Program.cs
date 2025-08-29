using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
/*using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
*/

class Program
{
	static async Task Main()
	{
		/*
		var avatarUrl = "https://avatars.githubusercontent.com/u/583231";
		using var client = new HttpClient();

		byte[]? avatarBytes = null;
		try
		{
			avatarBytes = await client.GetByteArrayAsync(avatarUrl);
		}
		catch
		{
		}

		using var avatar = avatarBytes != null
			? Image.Load(avatarBytes)
			: new Image<Rgba32>(128, 128, Color.DimGray);

		avatar.Mutate(x => x.Resize(new ResizeOptions
		{
			Size = new Size(128, 128),
			Mode = ResizeMode.Crop
		}));

		using var img = new Image<Rgba32>(820, 220, Color.Black);

		FontFamily family;
		if (!SystemFonts.TryGet("Arial", out family))
			family = SystemFonts.Families.First();
		var font = family.CreateFont(24);

		img.Mutate(ctx =>
		{
			ctx.DrawImage(avatar, new Point(20, 20), 1f);

			ctx.DrawText("Herr Doctor", font, Color.White, new PointF(170, 40));
			ctx.DrawText("Level 49", font, Color.White, new PointF(170, 80));
			ctx.DrawText("Rank 1", font, Color.White, new PointF(640, 40));
			ctx.DrawText("114.66k / 117.39k XP", font, Color.White, new PointF(480, 150));

			var barX = 170f;
			var barY = 140f;
			var barWidth = 560f;
			var barHeight = 28f;
			var progress = (float)Math.Clamp(114.66 / 117.39, 0.0, 1.0);
			var barColor = Color.ParseHex("#D7A037");

			ctx.Fill(Color.ParseHex("#2A2213"), new RectangleF(barX, barY, barWidth, barHeight));
			ctx.Fill(barColor, new RectangleF(barX, barY, barWidth * progress, barHeight));
		});

		await img.SaveAsPngAsync("rankcard.png");
		*/
	}
}
