using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BankMore.ContaCorrente.Infrastructure.Services;

public interface IKafkaService
{
    Task PublishAsync<T>(string topic, T message);
}

public class KafkaService : IKafkaService
{
    private readonly IProducerAccessor _producerAccessor;
    private readonly IConfiguration _configuration;

    public KafkaService(IProducerAccessor producerAccessor, IConfiguration configuration)
    {
        _producerAccessor = producerAccessor;
        _configuration = configuration;
    }

    public async Task PublishAsync<T>(string topic, T message)
    {
        var producer = _producerAccessor.GetProducer("default");
        var json = JsonSerializer.Serialize(message);
        await producer.ProduceAsync(topic, Guid.NewGuid().ToString(), json);
    }
}
