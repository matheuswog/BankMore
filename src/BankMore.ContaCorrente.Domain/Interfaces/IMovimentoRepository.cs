using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IMovimentoRepository
{
    Task<Movimento> InserirAsync(Movimento movimento);
    Task<decimal> CalcularSaldoAsync(string idContaCorrente);
    Task<List<Movimento>> ObterMovimentosPorContaAsync(string idContaCorrente);
}
