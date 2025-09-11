using MediatR;
using BankMore.Tarifa.Domain.Commands;
using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BankMore.Tarifa.Domain.Handlers;

public class ProcessarTarifaHandler : IRequestHandler<ProcessarTarifaCommand, Result<bool>>
{
    private readonly ITarifaRepository _tarifaRepository;
    private readonly IConfiguration _configuration;

    public ProcessarTarifaHandler(ITarifaRepository tarifaRepository, IConfiguration configuration)
    {
        _tarifaRepository = tarifaRepository;
        _configuration = configuration;
    }

    public async Task<Result<bool>> Handle(ProcessarTarifaCommand request, CancellationToken cancellationToken)
    {
        if (await _tarifaRepository.ExisteIdentificacaoTransferenciaAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true);
        }

        var valorTarifa = 2.00m; // TODO: Ler do appsettings

        var tarifa = new Domain.Entities.Tarifa
        {
            ContaCorrenteId = request.ContaCorrenteId,
            ValorTarifado = valorTarifa,
            DataTarifacao = DateTime.UtcNow,
            Descricao = $"Tarifa de transferência - {request.ValorTransferencia:C}",
            IdentificacaoTransferencia = request.IdentificacaoRequisicao
        };

        await _tarifaRepository.InserirAsync(tarifa);

        await EnviarEventoTarifaRealizada(tarifa);

        return Result<bool>.SucessoResultado(true);
    }

    private async Task EnviarEventoTarifaRealizada(Entities.Tarifa tarifa)
    {
        try
        {
            // TODO: Implementar Kafka
            Console.WriteLine($"Tarifa processada: Conta {tarifa.ContaCorrenteId} - {tarifa.ValorTarifado:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar evento para Kafka: {ex.Message}");
        }
    }
}
