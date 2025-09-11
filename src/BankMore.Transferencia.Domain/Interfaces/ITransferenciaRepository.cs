using BankMore.Transferencia.Domain.Entities;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task<Entities.Transferencia> InserirAsync(Entities.Transferencia transferencia);
    Task<bool> ExisteIdentificacaoRequisicaoAsync(string identificacaoRequisicao);
    Task<Entities.Transferencia?> ObterPorIdAsync(int id);
}
