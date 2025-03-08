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
    int backoffDelay = 300000; // Default wait time for rate limit hit (300 sec)

    while (!stoppingToken.IsCancellationRequested)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        try
        {
            // Normal rate limiting
            await RateLimiter.WaitAsync(stoppingToken);

            // Define the search parameters
            string leagueId = "Standard";
            string[] haveCurrencies = new[] { "divine", "chaos" };
            string wantCurrency = Essence.EssenceOfDelirium.GetEnumMemberValue();
            int minimum = 10;

            _logger.LogInformation("League: {leagueId}, Have: {haveCurrency}, Want: {wantCurrency}, Minimum: {minimum}", 
                leagueId, haveCurrencies, wantCurrency, minimum);

            // Execute the API request
            var response = await _tradeService.SearchCurrencyExchangeAsync(leagueId, haveCurrencies, wantCurrency, minimum);

            // Reset backoff delay if request was successful
            backoffDelay = 10000;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            // Rate limit hit! Apply a backoff delay
            _logger.LogWarning("Rate limit exceeded! Applying backoff...");

            // Try to get the `Retry-After` header from response
            int waitTime = backoffDelay; // Default backoff

            if (ex.Data.Contains("Retry-After"))
            {
                waitTime = (int)ex.Data["Retry-After"] * 1000; // Convert to milliseconds
                _logger.LogWarning("⏳ API says wait {WaitTime} ms", waitTime);
            }
            else
            {
                _logger.LogWarning("No Retry-After header found. Using backoff: {WaitTime} ms", waitTime);
                backoffDelay = Math.Min(backoffDelay * 2, 3000000); // Exponential backoff (max 3000 sec)
            }

            await Task.Delay(waitTime, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing bulk trade search");
        }
    }
}

}
