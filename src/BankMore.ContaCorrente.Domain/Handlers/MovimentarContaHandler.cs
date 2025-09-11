using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, Result<bool>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public MovimentarContaHandler(IContaCorrenteRepository contaRepository, IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<Result<bool>> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe movimentação com esta identificação (idempotência)
        if (await _movimentoRepository.ExisteIdentificacaoRequisicaoAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true); // Idempotência - já processado
        }

        // Buscar conta origem
        var contaOrigem = await _contaRepository.ObterPorIdAsync(request.ContaCorrenteId);
        if (contaOrigem == null)
        {
            return Result<bool>.ErroResultado("Conta não encontrada", TipoFalha.InvalidAccount.ToString());
        }

        // Verificar se conta está ativa
        if (!contaOrigem.Ativo)
        {
            return Result<bool>.ErroResultado("Conta inativa", TipoFalha.InactiveAccount.ToString());
        }

        // Validar valor
        if (request.Valor <= 0)
        {
            return Result<bool>.ErroResultado("Valor deve ser maior que zero", TipoFalha.InvalidValue.ToString());
        }

        // Validar tipo de movimento
        if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
        {
            return Result<bool>.ErroResultado("Tipo de movimento inválido", TipoFalha.InvalidType.ToString());
        }

        // Se for débito para conta diferente da logada, não permitir
        if (request.TipoMovimento == "D" && request.ContaCorrenteDestinoId.HasValue && 
            request.ContaCorrenteDestinoId.Value != request.ContaCorrenteId)
        {
            return Result<bool>.ErroResultado("Débito só pode ser realizado na própria conta", TipoFalha.InvalidType.ToString());
        }

        // Se for crédito para conta diferente, verificar se conta destino existe e está ativa
        if (request.TipoMovimento == "C" && request.ContaCorrenteDestinoId.HasValue)
        {
            var contaDestino = await _contaRepository.ObterPorIdAsync(request.ContaCorrenteDestinoId.Value);
            if (contaDestino == null)
            {
                return Result<bool>.ErroResultado("Conta destino não encontrada", TipoFalha.InvalidAccount.ToString());
            }
            if (!contaDestino.Ativo)
            {
                return Result<bool>.ErroResultado("Conta destino inativa", TipoFalha.InactiveAccount.ToString());
            }
        }

        // Criar movimento
        var movimento = new Movimento
        {
            IdentificacaoRequisicao = request.IdentificacaoRequisicao,
            ContaCorrenteId = request.ContaCorrenteId,
            TipoMovimento = request.TipoMovimento,
            Valor = request.Valor,
            DataMovimento = DateTime.UtcNow,
            Descricao = $"Movimentação {request.TipoMovimento} - {request.Valor:C}"
        };

        await _movimentoRepository.InserirAsync(movimento);

        return Result<bool>.SucessoResultado(true);
    }
}
