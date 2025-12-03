using KafkaFlow;
using BankMore.Tarifa.Domain.Events;
using MediatR;
using BankMore.ContaCorrente.Domain.Commands;

namespace BankMore.ContaCorrente.API.Handlers;

public class TarifaRealizadaHandler : IMessageHandler<TarifaRealizadaEvent>
{
    private readonly IMediator _mediator;

    public TarifaRealizadaHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(IMessageContext context, TarifaRealizadaEvent message)
    {
        var command = new MovimentarContaCommand
        {
            IdentificacaoRequisicao = $"TARIFA_{message.IdentificacaoRequisicaoTransferencia}",
            IdContaCorrente = message.IdContaCorrente,
            Valor = message.ValorTarifado,
            TipoMovimento = "D"
        };

        await _mediator.Send(command);
    }
}

