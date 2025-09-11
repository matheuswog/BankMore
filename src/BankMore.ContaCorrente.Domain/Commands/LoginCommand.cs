using MediatR;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Identificacao { get; set; } = string.Empty; // CPF ou número da conta
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ContaCorrenteId { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
}
