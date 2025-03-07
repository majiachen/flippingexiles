namespace WorkerService1.BulkTradeService;
public interface ITradeApiClient
{
    Task<TradeResponse> ExecuteBulkSearchAsync(string leagueId, string haveCurrency, string wantCurrency, int minimum);
    // Add other API methods as needed
}