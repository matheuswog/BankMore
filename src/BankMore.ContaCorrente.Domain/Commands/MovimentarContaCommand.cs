using MediatR;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Commands;

public class MovimentarContaCommand : IRequest<Result<bool>>
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public int? NumeroContaDestino { get; set; }
    public decimal Valor { get; set; }
    public string TipoMovimento { get; set; } = string.Empty;
}
