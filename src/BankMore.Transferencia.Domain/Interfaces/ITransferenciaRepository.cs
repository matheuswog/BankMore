using BankMore.Transferencia.Domain.Entities;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task<Entities.Transferencia> InserirAsync(Entities.Transferencia transferencia);
    Task<Entities.Transferencia?> ObterPorIdAsync(string idTransferencia);
}
