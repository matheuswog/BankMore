using MediatR;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Commands;

public class InativarContaCommand : IRequest<Result<bool>>
{
    public int ContaCorrenteId { get; set; }
    public string Senha { get; set; } = string.Empty;
}
