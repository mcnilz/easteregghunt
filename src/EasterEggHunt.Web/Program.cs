using EasterEggHunt.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure services
WebApplicationHostBuilder.ConfigureServices(builder);

var app = builder.Build();

// Configure pipeline
WebApplicationHostBuilder.ConfigurePipeline(app);

app.Run();
