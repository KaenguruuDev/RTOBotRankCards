namespace RTOSharpRankCards;

public class Startup
{
	private readonly string _indexTemplate = File.ReadAllText("index.html");

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
			endpoints.MapGet("/cards/{username}", GetCard);
			endpoints.MapPost("/card/new", CreateCard);
		});
	}

	private async Task GetIndex(HttpContext context)
		=> await context.Response.WriteAsync(_indexTemplate);

	private async Task GetCard(HttpContext context, string username)
	{
		
	}
	
	private async Task CreateCard(HttpContext context)
	{
	}
}