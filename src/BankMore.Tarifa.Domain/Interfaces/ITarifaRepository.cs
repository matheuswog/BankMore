using BankMore.Tarifa.Domain.Entities;

namespace BankMore.Tarifa.Domain.Interfaces;

public interface ITarifaRepository
{
    Task<Entities.Tarifa> InserirAsync(Entities.Tarifa tarifa);
}
