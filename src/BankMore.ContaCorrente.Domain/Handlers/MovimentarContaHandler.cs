using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using System.Text.Json;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, Result<bool>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;

    public MovimentarContaHandler(IContaCorrenteRepository contaRepository, IMovimentoRepository movimentoRepository, IIdempotenciaRepository idempotenciaRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
        _idempotenciaRepository = idempotenciaRepository;
    }

    public async Task<Result<bool>> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        if (await _idempotenciaRepository.ExisteChaveAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true);
        }

        var contaOrigem = await _contaRepository.ObterPorIdContaCorrenteAsync(request.IdContaCorrente);
        if (contaOrigem == null)
        {
            return Result<bool>.ErroResultado("Conta não encontrada", TipoFalha.InvalidAccount.ToString());
        }

        if (contaOrigem.Ativo != 1)
        {
            return Result<bool>.ErroResultado("Conta inativa", TipoFalha.InactiveAccount.ToString());
        }

        if (request.Valor <= 0)
        {
            return Result<bool>.ErroResultado("Valor deve ser maior que zero", TipoFalha.InvalidValue.ToString());
        }

        if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
        {
            return Result<bool>.ErroResultado("Tipo de movimento inválido", TipoFalha.InvalidType.ToString());
        }

        if (request.TipoMovimento == "D" && request.NumeroContaDestino.HasValue)
        {
            var contaDestino = await _contaRepository.ObterPorNumeroContaAsync(request.NumeroContaDestino.Value);
            if (contaDestino != null && contaDestino.IdContaCorrente != request.IdContaCorrente)
            {
                return Result<bool>.ErroResultado("Débito só pode ser realizado na própria conta", TipoFalha.InvalidType.ToString());
            }
        }

        if (request.TipoMovimento == "C" && request.NumeroContaDestino.HasValue)
        {
            var contaDestino = await _contaRepository.ObterPorNumeroContaAsync(request.NumeroContaDestino.Value);
            if (contaDestino == null)
            {
                return Result<bool>.ErroResultado("Conta destino não encontrada", TipoFalha.InvalidAccount.ToString());
            }
            if (contaDestino.Ativo != 1)
            {
                return Result<bool>.ErroResultado("Conta destino inativa", TipoFalha.InactiveAccount.ToString());
            }
            if (contaDestino.IdContaCorrente != request.IdContaCorrente)
            {
                request.IdContaCorrente = contaDestino.IdContaCorrente;
            }
        }

        var movimento = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            TipoMovimento = request.TipoMovimento,
            Valor = request.Valor,
            DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };

        await _movimentoRepository.InserirAsync(movimento);

        var requisicao = JsonSerializer.Serialize(request);
        var resultado = JsonSerializer.Serialize(new { Sucesso = true });
        await _idempotenciaRepository.SalvarAsync(request.IdentificacaoRequisicao, requisicao, resultado);

        return Result<bool>.SucessoResultado(true);
    }
}
