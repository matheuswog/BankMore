using MediatR;
using BankMore.Transferencia.Domain.Commands;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Domain.Events;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace BankMore.Transferencia.Domain.Handlers;

public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaCommand, Result<bool>>
{
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly IHttpClientService _httpClientService;
    private readonly IConfiguration _configuration;
    private readonly ITransferenciaEventProducer _kafkaProducer;

    public EfetuarTransferenciaHandler(
        ITransferenciaRepository transferenciaRepository,
        IIdempotenciaRepository idempotenciaRepository,
        IHttpClientService httpClientService,
        IConfiguration configuration,
        ITransferenciaEventProducer kafkaProducer)
    {
        _transferenciaRepository = transferenciaRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _httpClientService = httpClientService;
        _configuration = configuration;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<bool>> Handle(EfetuarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        if (await _idempotenciaRepository.ExisteChaveAsync(request.IdentificacaoRequisicao))
        {
            return Result<bool>.SucessoResultado(true);
        }

        if (request.Valor <= 0)
        {
            return Result<bool>.ErroResultado("Valor deve ser maior que zero", "INVALID_VALUE");
        }

        var contaOrigemValida = await ValidarContaPorId(request.IdContaCorrenteOrigem);
        if (!contaOrigemValida.Sucesso)
        {
            return Result<bool>.ErroResultado(contaOrigemValida.Erro?.Mensagem ?? "Erro ao validar conta origem", 
                contaOrigemValida.Erro?.TipoFalha ?? "INVALID_ACCOUNT");
        }

        var contaDestinoValida = await ValidarContaPorNumero(request.NumeroContaDestino, request.TokenJwt);
        if (!contaDestinoValida.Sucesso)
        {
            return Result<bool>.ErroResultado(contaDestinoValida.Erro?.Mensagem ?? "Erro ao validar conta destino", 
                contaDestinoValida.Erro?.TipoFalha ?? "INVALID_ACCOUNT");
        }

        var idContaDestino = await ObterIdContaPorNumero(request.NumeroContaDestino, request.TokenJwt);
        if (string.IsNullOrEmpty(idContaDestino))
        {
            return Result<bool>.ErroResultado("Conta destino não encontrada", "INVALID_ACCOUNT");
        }

        if (idContaDestino == request.IdContaCorrenteOrigem)
        {
            return Result<bool>.ErroResultado("Não é possível transferir para a mesma conta", "INVALID_ACCOUNT");
        }

        var debitoResult = await RealizarMovimentacao(request.IdContaCorrenteOrigem, request.Valor, "D", request.IdentificacaoRequisicao, request.TokenJwt);
        if (!debitoResult.Sucesso)
        {
            return Result<bool>.ErroResultado(debitoResult.Erro?.Mensagem ?? "Erro ao realizar débito", 
                debitoResult.Erro?.TipoFalha ?? "TRANSFER_ERROR");
        }

        try
        {
            var creditoResult = await RealizarMovimentacao(idContaDestino, request.Valor, "C", request.IdentificacaoRequisicao, request.TokenJwt, request.NumeroContaDestino);
            if (!creditoResult.Sucesso)
            {
                await RealizarMovimentacao(request.IdContaCorrenteOrigem, request.Valor, "C", $"{request.IdentificacaoRequisicao}_ESTORNO", request.TokenJwt);
                
                return Result<bool>.ErroResultado(creditoResult.Erro?.Mensagem ?? "Erro ao realizar crédito", 
                    creditoResult.Erro?.TipoFalha ?? "TRANSFER_ERROR");
            }

            var transferencia = new Entities.Transferencia
            {
                IdTransferencia = Guid.NewGuid().ToString(),
                IdContaCorrenteOrigem = request.IdContaCorrenteOrigem,
                IdContaCorrenteDestino = idContaDestino,
                Valor = request.Valor,
                DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy")
            };

            await _transferenciaRepository.InserirAsync(transferencia);

            var requisicao = JsonSerializer.Serialize(request);
            var resultado = JsonSerializer.Serialize(transferencia);
            await _idempotenciaRepository.SalvarAsync(request.IdentificacaoRequisicao, requisicao, resultado);

            await _kafkaProducer.ProduzirEventoAsync(new TransferenciaRealizadaEvent
            {
                IdentificacaoRequisicao = request.IdentificacaoRequisicao,
                IdContaCorrenteOrigem = request.IdContaCorrenteOrigem,
                ValorTransferencia = request.Valor,
                DataHora = DateTime.UtcNow
            });

            return Result<bool>.SucessoResultado(true);
        }
        catch (Exception ex)
        {
            await RealizarMovimentacao(request.IdContaCorrenteOrigem, request.Valor, "C", $"{request.IdentificacaoRequisicao}_ESTORNO", request.TokenJwt);
            
            return Result<bool>.ErroResultado($"Erro interno: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    private async Task<Result<bool>> ValidarContaPorId(string idContaCorrente)
    {
        return Result<bool>.SucessoResultado(true);
    }

    private async Task<Result<bool>> ValidarContaPorNumero(int numeroConta, string? token = null)
    {
        try
        {
            var response = await _httpClientService.GetAsync($"/api/conta-corrente/validar/{numeroConta}", token);
            
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

    private async Task<string?> ObterIdContaPorNumero(int numeroConta, string? token = null)
    {
        try
        {
            var response = await _httpClientService.GetAsync($"/api/conta-corrente/obter-id/{numeroConta}", token);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                if (result.TryGetProperty("idContaCorrente", out var idProperty))
                {
                    return idProperty.GetString();
                }
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Result<bool>> RealizarMovimentacao(string idContaCorrente, decimal valor, string tipo, string identificacaoRequisicao, string? token = null, int? numeroConta = null)
    {
        try
        {
            var requestBody = new
            {
                IdentificacaoRequisicao = identificacaoRequisicao,
                NumeroConta = numeroConta,
                NumeroContaDestino = numeroConta,
                Valor = valor,
                TipoMovimento = tipo
            };

            var response = await _httpClientService.PostAsync("/api/conta-corrente/movimentar", requestBody, token);
            
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
}
