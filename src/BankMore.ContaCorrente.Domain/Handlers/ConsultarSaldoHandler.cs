using MediatR;
using BankMore.ContaCorrente.Domain.Queries;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Infrastructure.Repositories;

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
        // Buscar conta
        var conta = await _contaRepository.ObterPorIdAsync(request.ContaCorrenteId);

        if (conta == null)
        {
            return Result<SaldoResponse>.ErroResultado("Conta não encontrada", TipoFalha.InvalidAccount.ToString());
        }

        // Verificar se conta está ativa
        if (!conta.Ativo)
        {
            return Result<SaldoResponse>.ErroResultado("Conta inativa", TipoFalha.InactiveAccount.ToString());
        }

        // Calcular saldo
        var saldo = await _movimentoRepository.CalcularSaldoAsync(request.ContaCorrenteId);

        var response = new SaldoResponse(conta.NumeroConta, conta.NomeTitular, saldo);

        return Result<SaldoResponse>.SucessoResultado(response);
    }
}
