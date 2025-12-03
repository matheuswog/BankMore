namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IIdempotenciaRepository
{
    Task<bool> ExisteChaveAsync(string chaveIdempotencia);
    Task SalvarAsync(string chaveIdempotencia, string requisicao, string resultado);
}

