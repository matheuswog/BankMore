using BankMore.Tarifa.Domain.Entities;

namespace BankMore.Tarifa.Infrastructure.Repositories;

public interface ITarifaRepository
{
    Task<Tarifa> InserirAsync(Tarifa tarifa);
    Task<bool> ExisteIdentificacaoTransferenciaAsync(string identificacaoTransferencia);
}
