using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.Application.Interfaces;
using Products.Application.UseCases;
using Products.Domain.Interfaces;
using Products.Infrastructure.AI;
using Products.Infrastructure.Caching;
using Products.Infrastructure.Repositories;

var host = new HostBuilder()
 .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Add appsettings.json
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        
      // Map Azure Functions environment variables to expected configuration format
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AzureOpenAI:Endpoint"] = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
            ["AzureOpenAI:Key"] = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"),
     ["AzureOpenAI:Deployment"] = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
["Redis:Connection"] = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
        }!);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Clean Architecture - Register layers from inside out
  
        // Application layer - Use Cases
        services.AddScoped<IProcessQueryUseCase, ProcessQueryUseCase>();

        // Infrastructure layer - Implementations
        services.AddSingleton<IBearingRepository, JsonBearingRepository>();
  services.AddSingleton<IAiExtractionService, AzureOpenAiExtractionService>();
        services.AddSingleton<ICacheService, RedisCacheService>();
    })
    .Build();

host.Run();
