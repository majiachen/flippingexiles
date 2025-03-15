using Confluent.Kafka;
using Newtonsoft.Json;

namespace WorkerService1.BulkTradeService.KafkaServices;

public class KafkaProducerService : IDisposable
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IProducer<string, string> _producer;
    
    public KafkaProducerService(ILogger<KafkaProducerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var config = new ProducerConfig
        {
            BootstrapServers = "kafka1:19092,kafka2:19093,kafka3:19094",    
            Debug = "broker,topic,metadata",
            BrokerAddressFamily = BrokerAddressFamily.V4,
            // Important - tell client to only use bootstrap servers
            ApiVersionFallbackMs = 0,
            SocketKeepaliveEnable = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task SendMessageAsync(string topic, string key, object message)
    {
        try
        {
            var enrichedMessage = new
            {
                Timestamp = DateTime.Now, // Add timestamp for freshness tracking
                Data = message
            };

            var messageValue = JsonConvert.SerializeObject(enrichedMessage);
            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = messageValue,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            await _producer.ProduceAsync(topic, kafkaMessage);
            _logger.LogDebug("Sent message to Kafka topic '{Topic}': Key={Key}, Timestamp={Timestamp}", topic, key, kafkaMessage.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to Kafka topic '{Topic}' for key: {Key}", topic, key);
        }
    }


    public void Dispose()
    {
        _producer.Dispose();
    }
}