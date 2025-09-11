using MediatR;
using BankMore.Transferencia.Domain.Commands;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BankMore.Transferencia.Domain.Handlers;

public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaCommand, Result<bool>>
{
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EfetuarTransferenciaHandler(
        ITransferenciaRepository transferenciaRepository, 
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _transferenciaRepository = transferenciaRepository;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Result<bool>> Handle(EfetuarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe transferência com esta identificação (idempotência)
        if (await _transferenciaRepository.ExisteIdentificacaoRequisicaoAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true); // Idempotência - já processado
        }

        // Validar valor
        if (request.Valor <= 0)
        {
            return Result<bool>.ErroResultado("Valor deve ser maior que zero", "INVALID_VALUE");
        }

        // Verificar se conta origem existe e está ativa (via API ContaCorrente)
        var contaOrigemValida = await ValidarConta(request.ContaOrigemId);
        if (!contaOrigemValida.Sucesso)
        {
            return Result<bool>.ErroResultado(contaOrigemValida.Erro?.Mensagem ?? "Erro ao validar conta origem", 
                contaOrigemValida.Erro?.TipoFalha ?? "INVALID_ACCOUNT");
        }

        // Verificar se conta destino existe e está ativa (via API ContaCorrente)
        var contaDestinoValida = await ValidarConta(request.ContaDestinoId);
        if (!contaDestinoValida.Sucesso)
        {
            return Result<bool>.ErroResultado(contaDestinoValida.Erro?.Mensagem ?? "Erro ao validar conta destino", 
                contaDestinoValida.Erro?.TipoFalha ?? "INVALID_ACCOUNT");
        }

        // Realizar débito na conta origem
        var debitoResult = await RealizarMovimentacao(request.ContaOrigemId, request.Valor, "D", request.IdentificacaoRequisicao);
        if (!debitoResult.Sucesso)
        {
            return Result<bool>.ErroResultado(debitoResult.Erro?.Mensagem ?? "Erro ao realizar débito", 
                debitoResult.Erro?.TipoFalha ?? "TRANSFER_ERROR");
        }

        try
        {
            // Realizar crédito na conta destino
            var creditoResult = await RealizarMovimentacao(request.ContaDestinoId, request.Valor, "C", request.IdentificacaoRequisicao);
            if (!creditoResult.Sucesso)
            {
                // Estorno na conta origem
                await RealizarMovimentacao(request.ContaOrigemId, request.Valor, "C", $"{request.IdentificacaoRequisicao}_ESTORNO");
                
                return Result<bool>.ErroResultado(creditoResult.Erro?.Mensagem ?? "Erro ao realizar crédito", 
                    creditoResult.Erro?.TipoFalha ?? "TRANSFER_ERROR");
            }

            // Registrar transferência
            var transferencia = new Entities.Transferencia
            {
                IdentificacaoRequisicao = request.IdentificacaoRequisicao,
                ContaOrigemId = request.ContaOrigemId,
                ContaDestinoId = request.ContaDestinoId,
                Valor = request.Valor,
                DataTransferencia = DateTime.UtcNow,
                Descricao = $"Transferência de {request.Valor:C}",
                Processada = true
            };

            await _transferenciaRepository.InserirAsync(transferencia);

            // Enviar evento para Kafka (opcional)
            await EnviarEventoTransferenciaRealizada(request);

            return Result<bool>.SucessoResultado(true);
        }
        catch (Exception ex)
        {
            // Estorno na conta origem em caso de erro
            await RealizarMovimentacao(request.ContaOrigemId, request.Valor, "C", $"{request.IdentificacaoRequisicao}_ESTORNO");
            
            return Result<bool>.ErroResultado($"Erro interno: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    private async Task<Result<bool>> ValidarConta(int contaId)
    {
        try
        {
            var baseUrl = _configuration["ContaCorrenteApi:BaseUrl"];
            var response = await _httpClient.GetAsync($"{baseUrl}/api/conta-corrente/validar/{contaId}");
            
            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.SucessoResultado(true);
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var erro = JsonSerializer.Deserialize<ErroResponse>(content);
            
            return Result<bool>.ErroResultado(erro?.Mensagem ?? "Erro ao validar conta", 
                erro?.TipoFalha ?? "INVALID_ACCOUNT");
        }
        catch (Exception ex)
        {
            return Result<bool>.ErroResultado($"Erro ao validar conta: {ex.Message}", "VALIDATION_ERROR");
        }
    }

    private async Task<Result<bool>> RealizarMovimentacao(int contaId, decimal valor, string tipo, string identificacao)
    {
        try
        {
            var baseUrl = _configuration["ContaCorrenteApi:BaseUrl"];
            var request = new
            {
                IdentificacaoRequisicao = identificacao,
                ContaCorrenteId = contaId,
                Valor = valor,
                TipoMovimento = tipo
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl}/api/conta-corrente/movimentar", content);
            
            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.SucessoResultado(true);
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var erro = JsonSerializer.Deserialize<ErroResponse>(responseContent);
            
            return Result<bool>.ErroResultado(erro?.Mensagem ?? "Erro ao realizar movimentação", 
                erro?.TipoFalha ?? "MOVEMENT_ERROR");
        }
        catch (Exception ex)
        {
            return Result<bool>.ErroResultado($"Erro ao realizar movimentação: {ex.Message}", "MOVEMENT_ERROR");
        }
    }

    private async Task EnviarEventoTransferenciaRealizada(EfetuarTransferenciaCommand request)
    {
        try
        {
            // Kafka service será injetado via DI
            // Por enquanto, apenas log
            Console.WriteLine($"Transferência realizada: {request.IdentificacaoRequisicao} - {request.Valor:C}");
        }
        catch (Exception ex)
        {
            // Log do erro, mas não falha a transferência
            Console.WriteLine($"Erro ao enviar evento para Kafka: {ex.Message}");
        }
    }
}
