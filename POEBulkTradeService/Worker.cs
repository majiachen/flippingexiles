using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService1.BulkTradeService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITradeService _tradeService;

    public Worker(ILogger<Worker> logger, ITradeService tradeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                // Define the search parameters
                string leagueId = "Standard";
                string haveCurrency = "chaos";
                string wantCurrency = "divine";
                int minimum = 10;

                // Execute the bulk trade search using the service layer
                var result = await _tradeService.SearchCurrencyExchangeAsync(
                    leagueId, haveCurrency, wantCurrency, minimum);

                // Log the results
                _logger.LogInformation("Found {total} results", result.total);
                
                // Only log a summary of the results to avoid excessive logging
                if (result.result?.Values != null && result.result.Values.Any())
                {
                    _logger.LogInformation("Top 5 exchange rates:");
                    
                    foreach (var tradeResult in result.result.Values.Take(5))
                    {
                        // Extract the exchange rate information
                        var exchangeInfo = tradeResult.listing?.offers?.FirstOrDefault()?.exchange;
                        var itemInfo = tradeResult.listing?.offers?.FirstOrDefault()?.item;
                        
                        if (exchangeInfo != null && itemInfo != null)
                        {
                            _logger.LogInformation(
                                "Seller: {seller}, Rate: {amount} {currency} for {itemAmount} {itemCurrency}, Stock: {stock}",
                                tradeResult.listing.account.name,
                                exchangeInfo.amount,
                                exchangeInfo.currency,
                                itemInfo.amount,
                                itemInfo.currency,
                                itemInfo.stock);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("No results found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing bulk trade search");
            }

            // Wait for 5 minutes before running again
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}