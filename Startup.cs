using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RTOSharpRankCards;

public class Startup
{
	private readonly string _indexTemplate = File.ReadAllText("index.html");
	private readonly HttpClient _client = new();

	public void ConfigureServices(IServiceCollection services)
		=> services.AddControllers();

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
			app.UseDeveloperExceptionPage();

		app.UseRouting();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapGet("/", GetIndex);
			endpoints.MapGet("/cards/{path}", GetCard);
			endpoints.MapPost("/card/new", CreateCard);
		});
	}

	private async Task GetIndex(HttpContext context)
		=> await context.Response.WriteAsync(_indexTemplate);

	private async Task GetCard(HttpContext context, string path)
	{
		var userPath = GetSafeFilePath("cards", path);
		if (!File.Exists(userPath))
		{
			context.Response.StatusCode = 404;
			return;
		}

		context.Response.Headers.Append("Content-Disposition", $"attachment; filename={userPath}");
		context.Response.Headers.Append("Content-Type", $"image/png");
		await using var stream = File.OpenRead(userPath);
		await stream.CopyToAsync(context.Response.Body);
	}

	private async Task CreateCard(HttpContext context)
	{
		var data = await context.Request.ReadFromJsonAsync<CreateCardRequestData>();
		if (data is null)
		{
			context.Response.StatusCode = 400;
			await context.Response.WriteAsync("INVALID DATA: COULD NOT PARSE JSON");
			return;
		}

		var path = await CreateCardFromData(data);
		if (File.Exists(path))
		{
			await context.Response.WriteAsync(path);
		}
		else
		{
			context.Response.StatusCode = 400;
			await context.Response.WriteAsync($"INVALID DATA: INVALID RETURN PATH: {path}");
		}
	}

	private async Task<string> CreateCardFromData(CreateCardRequestData data)
	{
		using var img = new Image<Rgba32>(1326, 400, Color.Transparent);

		var collection = new FontCollection();
		var family = collection.Add("SmoochSans-Medium.ttf");
		var smallFont = family.CreateFont(48);
		var bigFont = family.CreateFont(62);

		byte[]? avatarBytes;
		try
		{
			avatarBytes = await _client.GetByteArrayAsync(data.AvatarUrl.Replace("128", "256"));
		}
		catch (Exception ex)
		{
			return ex.Message;
		}

		var avatar = Image.Load(avatarBytes);
		avatar.Mutate(x => x.Resize(new ResizeOptions
		{
			Size = new Size(256, 256),
			Mode = ResizeMode.Crop
		}));

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

			ctx.DrawText(data.Username, bigFont, Color.White, new PointF(365, 105));
			ctx.DrawText($"Level {data.Level}", smallFont, Color.White, new PointF(365, 190));

			var rankText = $"Rank {data.Rank}";
			var rankTextSize = TextMeasurer.MeasureSize(rankText, new TextOptions(bigFont));

			var xpToNextLevelText = $"{BeautifyXp(data.Xp)} / {BeautifyXp(data.XpToNextLevel)} XP";
			var xpToNextLevelTextSize = TextMeasurer.MeasureSize(xpToNextLevelText, new TextOptions(smallFont));

			ctx.DrawText(rankText, bigFont, Color.White, new PointF(1254 - rankTextSize.Width, 105));
			ctx.DrawText(xpToNextLevelText, smallFont, Color.White,
				new PointF(1254 - xpToNextLevelTextSize.Width, 190));

			const int barX = 365;
			const int barY = 252;
			const float barWidth = 889f;
			const float barHeight = 62f;
			var progress = data.Progress / 100f;
			var barColor = Color.ParseHex(data.CardColor);

			ctx.FillRoundedRectangle(barColor.Darken(0.8f), new RectangleF(barX, barY, barWidth, barHeight), 5);
			ctx.FillRoundedRectangle(barColor, new RectangleF(barX, barY, barWidth * progress, barHeight), 5);
		});

		await img.SaveAsPngAsync(GetSafeFilePath("cards", $"{data.GuildId}-{data.UserId}-{data.Level}-{data.Xp}"));
		return GetSafeFilePath("cards", $"{data.GuildId}-{data.UserId}-{data.Level}-{data.Xp}");
	}

	private static string GetSafeFilePath(string baseDirectory, string id, string extension = ".png")
	{
		return Path.Combine(baseDirectory, id + extension);
	}

	private static string BeautifyXp(int xp)
	{
		if (xp < 1000)
			return xp.ToString();

		return xp < 1000000 ? $"{MathF.Round(xp / 1000f, 2)}k" : $"{MathF.Round(xp / 1000000f, 2)}m";
	}

	private record CreateCardRequestData(
		string Username,
		string AvatarUrl,
		ulong GuildId,
		ulong UserId,
		int Xp,
		int Level,
		int Rank,
		int XpToNextLevel,
		int Progress,
		string CardColor);
}