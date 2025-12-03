using MediatR;
using BankMore.Tarifa.Domain.Commands;
using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Domain.Interfaces;
using BankMore.Tarifa.Domain.Events;
using Microsoft.Extensions.Configuration;

namespace BankMore.Tarifa.Domain.Handlers;

public class ProcessarTarifaHandler : IRequestHandler<ProcessarTarifaCommand, Result<bool>>
{
    private readonly ITarifaRepository _tarifaRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly IConfiguration _configuration;
    private readonly ITarifaEventProducer _kafkaProducer;

    public ProcessarTarifaHandler(ITarifaRepository tarifaRepository, IIdempotenciaRepository idempotenciaRepository, IConfiguration configuration, ITarifaEventProducer kafkaProducer)
    {
        _tarifaRepository = tarifaRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _configuration = configuration;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<bool>> Handle(ProcessarTarifaCommand request, CancellationToken cancellationToken)
    {
        if (await _idempotenciaRepository.ExisteChaveAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true);
        }

        var valorTarifa = request.ValorTransferencia > 0 
            ? request.ValorTransferencia 
            : decimal.Parse(_configuration["Tarifa:ValorTransferencia"] ?? "2.00");

        var tarifa = new Domain.Entities.Tarifa
        {
            IdTarifa = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            Valor = valorTarifa,
            DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy")
        };

        await _tarifaRepository.InserirAsync(tarifa);

        var requisicao = System.Text.Json.JsonSerializer.Serialize(request);
        var resultado = System.Text.Json.JsonSerializer.Serialize(tarifa);
        await _idempotenciaRepository.SalvarAsync(request.IdentificacaoRequisicao, requisicao, resultado);

        await _kafkaProducer.ProduzirEventoAsync(new TarifaRealizadaEvent
        {
            IdTarifa = tarifa.IdTarifa,
            IdContaCorrente = tarifa.IdContaCorrente,
            ValorTarifado = tarifa.Valor,
            IdentificacaoRequisicaoTransferencia = request.IdentificacaoRequisicao
        });

        return Result<bool>.SucessoResultado(true);
    }
}
