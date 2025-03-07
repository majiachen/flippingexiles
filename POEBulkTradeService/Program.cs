using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using WorkerService1.BulkTradeService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register the Cache singleton
        services.AddSingleton<Cache>();

        // Register RateLimiter
        services.AddSingleton(new RateLimiter(20, 60));
        
        // Register HttpClient
        services.AddHttpClient<PathOfExileApiClient>(client =>
        {
        });
        
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

await host.RunAsync();