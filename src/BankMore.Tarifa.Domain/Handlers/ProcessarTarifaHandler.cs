using MediatR;
using BankMore.Tarifa.Domain.Commands;
using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Infrastructure.Repositories;

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
        // Verificar se já existe tarifa para esta transferência (idempotência)
        if (await _tarifaRepository.ExisteIdentificacaoTransferenciaAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true); // Idempotência - já processado
        }

        // Obter valor da tarifa do appsettings
        var valorTarifa = _configuration.GetValue<decimal>("Tarifa:ValorTransferencia", 2.00m);

        // Criar tarifa
        var tarifa = new Tarifa
        {
            ContaCorrenteId = request.ContaCorrenteId,
            ValorTarifado = valorTarifa,
            DataTarifacao = DateTime.UtcNow,
            Descricao = $"Tarifa de transferência - {request.ValorTransferencia:C}",
            IdentificacaoTransferencia = request.IdentificacaoRequisicao
        };

        await _tarifaRepository.InserirAsync(tarifa);

        // Enviar evento para Kafka (opcional)
        await EnviarEventoTarifaRealizada(tarifa);

        return Result<bool>.SucessoResultado(true);
    }

    private async Task EnviarEventoTarifaRealizada(Tarifa tarifa)
    {
        try
        {
            var kafkaService = _configuration.GetService<BankMore.Tarifa.Infrastructure.Services.IKafkaService>();
            if (kafkaService != null)
            {
                var evento = new TarifaRealizadaEvent
                {
                    ContaCorrenteId = tarifa.ContaCorrenteId,
                    ValorTarifado = tarifa.ValorTarifado,
                    DataTarifacao = tarifa.DataTarifacao
                };

                await kafkaService.PublishAsync("tarifas-realizadas", evento);
            }
        }
        catch (Exception ex)
        {
            // Log do erro, mas não falha o processamento
            Console.WriteLine($"Erro ao enviar evento para Kafka: {ex.Message}");
        }
    }
}
