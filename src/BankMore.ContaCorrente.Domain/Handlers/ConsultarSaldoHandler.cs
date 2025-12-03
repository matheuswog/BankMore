using MediatR;
using BankMore.ContaCorrente.Domain.Queries;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class ConsultarSaldoHandler : IRequestHandler<ConsultarSaldoQuery, Result<SaldoResponse>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public ConsultarSaldoHandler(IContaCorrenteRepository contaRepository, IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<Result<SaldoResponse>> Handle(ConsultarSaldoQuery request, CancellationToken cancellationToken)
    {
        var conta = await _contaRepository.ObterPorIdContaCorrenteAsync(request.IdContaCorrente);

        if (conta == null)
        {
            return Result<SaldoResponse>.ErroResultado("Conta não encontrada", TipoFalha.InvalidAccount.ToString());
        }

        if (conta.Ativo != 1)
        {
            return Result<SaldoResponse>.ErroResultado("Conta inativa", TipoFalha.InactiveAccount.ToString());
        }

        var saldo = await _movimentoRepository.CalcularSaldoAsync(request.IdContaCorrente);

        var response = new SaldoResponse(conta.Numero.ToString(), conta.Nome, saldo);

        return Result<SaldoResponse>.SucessoResultado(response);
    }
}
