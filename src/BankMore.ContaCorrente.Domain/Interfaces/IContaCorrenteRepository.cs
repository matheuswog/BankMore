using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IContaCorrenteRepository
{
    Task<Entities.ContaCorrente?> ObterPorIdContaCorrenteAsync(string idContaCorrente);
    Task<Entities.ContaCorrente?> ObterPorNumeroContaAsync(int numero);
    Task<Entities.ContaCorrente?> ObterPorCpfOuNumeroContaAsync(string identificacao);
    Task<Entities.ContaCorrente> InserirAsync(Entities.ContaCorrente contaCorrente);
    Task AtualizarAsync(Entities.ContaCorrente contaCorrente);
    Task<bool> ExisteCpfAsync(string cpf);
    Task<bool> ExisteNumeroContaAsync(int numero);
}
