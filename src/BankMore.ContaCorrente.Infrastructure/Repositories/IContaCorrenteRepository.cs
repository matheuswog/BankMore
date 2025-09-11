using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public interface IContaCorrenteRepository
{
    Task<ContaCorrente?> ObterPorIdAsync(int id);
    Task<ContaCorrente?> ObterPorCpfAsync(string cpf);
    Task<ContaCorrente?> ObterPorNumeroContaAsync(string numeroConta);
    Task<ContaCorrente?> ObterPorCpfOuNumeroContaAsync(string identificacao);
    Task<ContaCorrente> InserirAsync(ContaCorrente contaCorrente);
    Task AtualizarAsync(ContaCorrente contaCorrente);
    Task<bool> ExisteCpfAsync(string cpf);
    Task<bool> ExisteNumeroContaAsync(string numeroConta);
}
