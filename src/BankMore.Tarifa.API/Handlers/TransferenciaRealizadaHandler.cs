using KafkaFlow;
using BankMore.Transferencia.Domain.Events;
using MediatR;
using BankMore.Tarifa.Domain.Commands;

namespace BankMore.Tarifa.API.Handlers;

public class TransferenciaRealizadaHandler : IMessageHandler<TransferenciaRealizadaEvent>
{
    private readonly IMediator _mediator;

    public TransferenciaRealizadaHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(IMessageContext context, TransferenciaRealizadaEvent message)
    {
        var command = new ProcessarTarifaCommand
        {
            IdentificacaoRequisicao = message.IdentificacaoRequisicao,
            IdContaCorrente = message.IdContaCorrenteOrigem,
            ValorTransferencia = message.ValorTransferencia
        };

        await _mediator.Send(command);
    }
}

