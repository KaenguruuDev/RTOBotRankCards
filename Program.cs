namespace RTOSharpRankCards;

sealed class Program
{
	static async Task Main(string[] args)
	{
		var host = Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
				webBuilder.UseUrls("http://localhost:2052");
			}).Build();
		await host.RunAsync();
	}
}