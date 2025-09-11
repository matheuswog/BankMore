using BankMore.Transferencia.Domain.Entities;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public interface ITransferenciaRepository
{
    Task<Transferencia> InserirAsync(Transferencia transferencia);
    Task<bool> ExisteIdentificacaoRequisicaoAsync(string identificacaoRequisicao);
    Task<Transferencia?> ObterPorIdAsync(int id);
}
