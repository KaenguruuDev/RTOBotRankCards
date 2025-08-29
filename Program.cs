using SixLabors.ImageSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RTOSharpRankCards;

sealed class Program
{
	static async Task Main(string[] args)
	{
		var host = Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>()).Build();
		await host.RunAsync();

		return;
		const string avatarUrl =
			"https://cdn.discordapp.com/avatars/772827007247319071/e749f88875364f7f73c31542896ab5b8.png?size=256";
		using var client = new HttpClient();

		byte[]? avatarBytes = null;
		try
		{
			avatarBytes = await client.GetByteArrayAsync(avatarUrl);
		}
		catch
		{
		}

		var avatar = avatarBytes != null ? Image.Load(avatarBytes) : new Image<Rgba32>(128, 128, Color.DimGray);

		avatar.Mutate(x => x.Resize(new ResizeOptions
		{
			Size = new Size(256, 256),
			Mode = ResizeMode.Crop
		}));

		using var img = new Image<Rgba32>(1326, 400, Color.Transparent);

		var collection = new FontCollection();
		var family = collection.Add("SmoochSans-Medium.ttf");
		var smallFont = family.CreateFont(48);
		var bigFont = family.CreateFont(62);

		img.Mutate(ctx =>
		{
			var avatarPos = new Point(72, 72);
			const int avatarSize = 256;
			const float cornerRadius = 20f;

			var rect = new RectangleF(0, 0, 1326, 400);
			ctx.FillRoundedRectangle(Color.Black, rect, cornerRadius);

			using var roundedAvatar =
				avatar.Clone(a => a.ConvertToAvatar(new Size(avatarSize, avatarSize), avatarSize / 2f));

			ctx.DrawImage(roundedAvatar, avatarPos, 1f);

			ctx.DrawText("Herr Doctor", bigFont, Color.White, new PointF(365, 105));
			ctx.DrawText("Level 49", smallFont, Color.White, new PointF(365, 190));

			var rankTextSize = TextMeasurer.MeasureSize("Rank 1", new TextOptions(bigFont));
			var xpToNextLevelSize = TextMeasurer.MeasureSize("114.66k / 117.39k XP", new TextOptions(smallFont));

			ctx.DrawText("Rank 1", bigFont, Color.White, new PointF(1254 - rankTextSize.Width, 105));
			ctx.DrawText("114.66k / 117.39k XP", smallFont, Color.White,
				new PointF(1254 - xpToNextLevelSize.Width, 190));

			const int barX = 365;
			const int barY = 252;
			const float barWidth = 889f;
			const float barHeight = 62f;
			var progress = (float)Math.Clamp(114.66 / 117.39, 0.0, 1.0);
			var barColor = Color.ParseHex("#D7A037");

			ctx.FillRoundedRectangle(barColor.Darken(0.8f), new RectangleF(barX, barY, barWidth, barHeight), 5);
			ctx.FillRoundedRectangle(barColor, new RectangleF(barX, barY, barWidth * progress, barHeight), 5);
		});

		await img.SaveAsPngAsync("rankcard.png");
	}
}