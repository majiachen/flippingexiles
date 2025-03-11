using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerService1.BulkTradeService;
using WorkerService1.BulkTradeService.Enums;
using WorkerService1.BulkTradeService.KafkaServices;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITradeService _tradeService;
    private readonly KafkaProducerService _kafkaProducer;

    public Worker(ILogger<Worker> logger, ITradeService tradeService, KafkaProducerService kafkaProducer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
        _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Worker started execution at: {Time}", DateTimeOffset.Now);

            var tradeItemTypes = GetEnumTypes();
            _logger.LogDebug("Discovered {EnumCount} enum types for trading: {EnumTypes}", 
                tradeItemTypes.Length, string.Join(", ", tradeItemTypes.Select(t => t.Name)));

            foreach (var tradeItemType in tradeItemTypes)
            {
                var items = GetEnumValues(tradeItemType);
                var topic = GetKafkaTopic(tradeItemType);

                _logger.LogDebug("Processing {ItemCount} trade items from {EnumType}, sending data to Kafka topic '{Topic}'", 
                    items.Length, tradeItemType.Name, topic);

                var tradeDataBatch = new List<object>();
                await _kafkaProducer.SendMessageAsync(topic, "trade-data-refresh", tradeDataBatch);
                _logger.LogDebug("test Trade data sent to Kafka topic '{Topic}' successfully.", topic);
                foreach (var item in items)
                {
                    try
                    {
                        await RateLimiter.WaitAsync(stoppingToken);
                        _logger.LogDebug("Rate limiter cleared, requesting trade data for {TradeItem}", item);

                        string leagueId = "Phrecia";
                        string[] haveCurrencies = { "divine", "chaos" };
                        int minimum = 10;

                        _logger.LogDebug("Searching for {TradeItem} in {LeagueId} league", item, leagueId);
                        var result = await _tradeService.SearchCurrencyExchangeAsync(leagueId, haveCurrencies, item, minimum);

                        tradeDataBatch.Add(new { Item = item, Data = result });
                        _logger.LogDebug("Collected trade data for {TradeItem}", item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while searching for trade item: {TradeItem}", item);
                    }
                }

                if (tradeDataBatch.Any())
                {
                    _logger.LogDebug("Completed processing {EnumType}. Sending data to Kafka topic '{Topic}'", tradeItemType.Name, topic);
                    await _kafkaProducer.SendMessageAsync(topic, "trade-data-refresh", tradeDataBatch);
                    _logger.LogDebug("Trade data sent to Kafka topic '{Topic}' successfully.", topic);
                }
            }

            _logger.LogDebug("Restarting cycle...");
        }
    }

    private static Type[] GetEnumTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsEnum && t.Namespace == "WorkerService1.BulkTradeService.Enums")
            .ToArray();
    }

    private string[] GetEnumValues(Type enumType)
    {
        var values = Enum.GetValues(enumType)
            .Cast<Enum>()
            .Select(e => e.GetEnumMemberValue())
            .ToArray();

        _logger.LogDebug("Extracted {ValueCount} values from enum {EnumType}: {Values}", 
            values.Length, enumType.Name, string.Join(", ", values));

        return values;
    }

    private static string GetKafkaTopic(Type enumType)
    {
        return enumType.Name.ToLower() switch
        {
            "essence" => "essence",
            "fossil" => "fossils",
            "scarab" => "scarabs",
            _ => "misc"
        };
    }
}
