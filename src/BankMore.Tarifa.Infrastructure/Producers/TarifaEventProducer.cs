using BankMore.Tarifa.Domain.Events;
using BankMore.Tarifa.Domain.Interfaces;
using KafkaFlow;

namespace BankMore.Tarifa.Infrastructure.Producers;

public class TarifaEventProducer : ITarifaEventProducer
{
    private readonly IMessageProducer<TarifaRealizadaEvent> _producer;

    public TarifaEventProducer(IMessageProducer<TarifaRealizadaEvent> producer)
    {
        _producer = producer;
    }

    public async Task ProduzirEventoAsync(TarifaRealizadaEvent evento)
    {
        await _producer.ProduceAsync(Guid.NewGuid().ToString(), evento);
    }
}

