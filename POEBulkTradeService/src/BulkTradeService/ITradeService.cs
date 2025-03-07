namespace WorkerService1.BulkTradeService;

public interface ITradeService
{
    Task<TradeResponse> SearchCurrencyExchangeAsync(string leagueId, string haveCurrency, string wantCurrency, int minimum);
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

    public async Task<TradeResponse> SearchCurrencyExchangeAsync(string leagueId, string haveCurrency, string wantCurrency, int minimum)
    {
        // Create a cache key based on the parameters
        var cacheKey = new { 
            Type = "BulkSearch", 
            League = leagueId, 
            Have = haveCurrency, 
            Want = wantCurrency, 
            Min = minimum 
        };

        // Check cache first
        var cachedResult = _cache.Get<TradeResponse>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Retrieved bulk trade search from cache for league: {LeagueId}", leagueId);
            return cachedResult;
        }

        // Call the API if not in cache
        var result = await _apiClient.ExecuteBulkSearchAsync(leagueId, haveCurrency, wantCurrency, minimum);
        
        // Cache the result (with a TTL of 5 minutes)
        _cache.Set(cacheKey, result, 300);
        
        return result;
    }
}
