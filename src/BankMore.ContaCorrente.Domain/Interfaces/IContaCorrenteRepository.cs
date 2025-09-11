using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IContaCorrenteRepository
{
    Task<Entities.ContaCorrente?> ObterPorIdAsync(int id);
    Task<Entities.ContaCorrente?> ObterPorCpfAsync(string cpf);
    Task<Entities.ContaCorrente?> ObterPorNumeroContaAsync(string numeroConta);
    Task<Entities.ContaCorrente?> ObterPorCpfOuNumeroContaAsync(string identificacao);
    Task<Entities.ContaCorrente> InserirAsync(Entities.ContaCorrente contaCorrente);
    Task AtualizarAsync(Entities.ContaCorrente contaCorrente);
    Task<bool> ExisteCpfAsync(string cpf);
    Task<bool> ExisteNumeroContaAsync(string numeroConta);
}
