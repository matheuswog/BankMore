using BankMore.Transferencia.Domain.Events;
using BankMore.Transferencia.Domain.Interfaces;
using KafkaFlow;

namespace BankMore.Transferencia.Infrastructure.Producers;

public class TransferenciaEventProducer : ITransferenciaEventProducer
{
    private readonly IMessageProducer<TransferenciaRealizadaEvent> _producer;

    public TransferenciaEventProducer(IMessageProducer<TransferenciaRealizadaEvent> producer)
    {
        _producer = producer;
    }

    public async Task ProduzirEventoAsync(TransferenciaRealizadaEvent evento)
    {
        await _producer.ProduceAsync(Guid.NewGuid().ToString(), evento);
    }
}

