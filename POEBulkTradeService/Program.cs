using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using WorkerService1.BulkTradeService;
using WorkerService1.BulkTradeService.KafkaServices;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register Kafka producer
        services.AddSingleton<KafkaProducerService>();

        // Register Trade-related services
        services.AddSingleton<Cache>();

        // **Register HttpClient**
        services.AddHttpClient<PathOfExileApiClient>();

        // Register services as singletons instead of scoped
        services.AddSingleton<ITradeApiClient, PathOfExileApiClient>();
        services.AddSingleton<ITradeService, TradeService>();

        // Register the worker service
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging((context, logging) =>
    {
        // Configure logging
        logging.ClearProviders();
        logging.AddConsole(); // Log to the console
        logging.AddDebug();   // Log to the debug output
    })
    .Build();

// **Pass the logger to RateLimiter**
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiter");
RateLimiter.InitializeLogger(logger);

await host.RunAsync();