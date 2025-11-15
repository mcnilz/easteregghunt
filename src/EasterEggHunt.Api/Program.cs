using EasterEggHunt.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ApiApplicationHostBuilder.ConfigureServices(builder);

var app = builder.Build();

// Configure pipeline
ApiApplicationHostBuilder.ConfigurePipeline(app);

app.Run();
