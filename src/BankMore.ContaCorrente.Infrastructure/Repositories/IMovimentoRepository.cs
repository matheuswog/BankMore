using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public interface IMovimentoRepository
{
    Task<Movimento> InserirAsync(Movimento movimento);
    Task<decimal> CalcularSaldoAsync(int contaCorrenteId);
    Task<bool> ExisteIdentificacaoRequisicaoAsync(string identificacaoRequisicao);
    Task<List<Movimento>> ObterMovimentosPorContaAsync(int contaCorrenteId);
}
