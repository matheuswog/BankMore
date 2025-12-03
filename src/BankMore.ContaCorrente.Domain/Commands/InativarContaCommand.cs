using MediatR;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Commands;

public class InativarContaCommand : IRequest<Result<bool>>
{
    public string IdContaCorrente { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
