using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService1.BulkTradeService;
using WorkerService1.BulkTradeService.Enums;

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
            _logger.LogDebug("Worker started execution at: {Time}", DateTimeOffset.Now);

            // Retrieve all enum types dynamically
            var tradeItemTypes = GetEnumTypes();

            _logger.LogDebug("Discovered {EnumCount} enum types for trading: {EnumTypes}", 
                tradeItemTypes.Length, string.Join(", ", tradeItemTypes.Select(t => t.Name)));

            foreach (var tradeItemType in tradeItemTypes)
            {
                var items = GetEnumValues(tradeItemType);

                _logger.LogDebug("Processing {ItemCount} trade items from {EnumType}", 
                    items.Length, tradeItemType.Name);

                foreach (var item in items)
                {
                    try
                    {
                        // Wait for rate limit before making request
                        _logger.LogDebug("Waiting for rate limiter before requesting trade data for {TradeItem}", item);
                        await RateLimiter.WaitAsync(stoppingToken);
                        _logger.LogDebug("Rate limiter cleared, proceeding with request for {TradeItem}", item);

                        // Define the search parameters
                        string leagueId = "Standard";
                        string[] haveCurrencies = { "divine", "chaos" }; // Example values
                        int minimum = 10;

                        _logger.LogDebug("Searching for {TradeItem} in {LeagueId} league", item, leagueId);

                        // Execute the API request
                        var result = await _tradeService.SearchCurrencyExchangeAsync(leagueId, haveCurrencies, item, minimum);

                        _logger.LogDebug("Found {TotalResults} results for {TradeItem}", result.total, item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while searching for trade item: {TradeItem}", item);
                    }
                }
            }

            _logger.LogDebug("Completed iteration through all trade items. Restarting cycle.");
        }
    }

    /// <summary>
    /// Retrieves all enum types dynamically from the assembly.
    /// </summary>
    private static Type[] GetEnumTypes()
    {
        var enumTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsEnum && t.Namespace == "WorkerService1.BulkTradeService.Enums") // Adjust namespace if needed
            .ToArray();

        return enumTypes;
    }

    /// <summary>
    /// Retrieves all enum values for a given enum type and converts them to their API-friendly values.
    /// </summary>
    private string[] GetEnumValues(Type enumType)
    {
        var values = Enum.GetValues(enumType)
            .Cast<Enum>()
            .Select(e =>
            {
                var value = e.GetEnumMemberValue(); // Get EnumMember value
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogWarning("Enum value {EnumValue} in {EnumType} does not have an EnumMember attribute, using default name.", e, enumType.Name);
                    return e.ToString(); // Use default enum name as fallback
                }
                return value;
            })
            .ToArray();

        _logger.LogDebug("Extracted {ValueCount} values from enum {EnumType}: {Values}", 
            values.Length, enumType.Name, string.Join(", ", values));

        return values;
    }

}
