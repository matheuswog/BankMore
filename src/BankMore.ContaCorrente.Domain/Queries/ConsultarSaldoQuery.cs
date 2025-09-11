using MediatR;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Queries;

public class ConsultarSaldoQuery : IRequest<Result<SaldoResponse>>
{
    public int ContaCorrenteId { get; set; }
}
