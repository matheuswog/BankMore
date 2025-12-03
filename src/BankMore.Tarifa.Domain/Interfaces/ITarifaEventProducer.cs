using BankMore.Tarifa.Domain.Events;

namespace BankMore.Tarifa.Domain.Interfaces;

public interface ITarifaEventProducer
{
    Task ProduzirEventoAsync(TarifaRealizadaEvent evento);
}

