using BankMore.Transferencia.Domain.Events;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaEventProducer
{
    Task ProduzirEventoAsync(TransferenciaRealizadaEvent evento);
}

