namespace WorkerService1.BulkTradeService;

public interface ITradeService
{
    Task<TradeResponse> SearchCurrencyExchangeAsync(string leagueId, string[] haveCurrency, string wantCurrency, int minimum);
    // Add other business-logic methods
}

public class TradeService : ITradeService
{
    private readonly ITradeApiClient _apiClient;
    private readonly ILogger<TradeService> _logger;
    private readonly Cache _cache;

    public TradeService(
        ITradeApiClient apiClient,
        ILogger<TradeService> logger,
        Cache cache)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<TradeResponse> SearchCurrencyExchangeAsync(string leagueId, string[] haveCurrency, string wantCurrency, int minimum)
    {

        // Call the API if not in cache
        var result = await _apiClient.ExecuteBulkSearchAsync(leagueId, haveCurrency, wantCurrency, minimum);
        
        
        return result;
    }
}
