using Microsoft.AspNetCore.Mvc;
using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Queries;
using BankMore.ContaCorrente.Domain.ValueObjects;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BankMore.ContaCorrente.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaCorrenteController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IContaCorrenteRepository _contaRepository;

    public ContaCorrenteController(IMediator mediator, IContaCorrenteRepository contaRepository)
    {
        _mediator = mediator;
        _contaRepository = contaRepository;
    }

    /// <summary>
    /// Cadastra uma nova conta corrente
    /// </summary>
    /// <param name="request">Dados para cadastro da conta</param>
    /// <returns>Número da conta criada</returns>
    [HttpPost("cadastrar")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarContaCorrenteRequest request)
    {
        var command = new CadastrarContaCorrenteCommand
        {
            Cpf = request.Cpf,
            Senha = request.Senha,
            NomeTitular = request.NomeTitular
        };

        var result = await _mediator.Send(command);

        if (result.Sucesso)
        {
            return Ok(result.Dados);
        }

        return BadRequest(result.Erro);
    }

    /// <summary>
    /// Realiza login na conta corrente
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <returns>Token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ErroResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Identificacao = request.Identificacao,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);

        if (result.Sucesso)
        {
            return Ok(result.Dados);
        }

        return Unauthorized(result.Erro);
    }

    /// <summary>
    /// Inativa uma conta corrente
    /// </summary>
    /// <param name="request">Dados para inativação</param>
    /// <returns>Confirmação de inativação</returns>
    [HttpPost("inativar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Inativar([FromBody] InativarContaRequest request)
    {
        var idContaCorrente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Forbid();
        }

        var command = new InativarContaCommand
        {
            IdContaCorrente = idContaCorrente,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);

        if (result.Sucesso)
        {
            return NoContent();
        }

        return BadRequest(result.Erro);
    }

    /// <summary>
    /// Realiza movimentação na conta corrente
    /// </summary>
    /// <param name="request">Dados da movimentação</param>
    /// <returns>Confirmação da movimentação</returns>
    [HttpPost("movimentar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Movimentar([FromBody] MovimentarContaRequest request)
    {
        var idContaCorrente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Forbid();
        }

        var numeroContaLogado = User.FindFirst("NumeroConta")?.Value;
        var idContaParaMovimentar = idContaCorrente;
        
        if (request.NumeroConta.HasValue)
        {
            var contaPorNumero = await ObterIdContaPorNumero(request.NumeroConta.Value);
            if (contaPorNumero != null)
            {
                if (numeroContaLogado != request.NumeroConta.Value.ToString() && request.TipoMovimento != "C")
                {
                    return BadRequest(new ErroResponse("Apenas o tipo crédito pode ser aceito caso o número da conta seja diferente do usuário logado", "INVALID_TYPE"));
                }
                idContaParaMovimentar = contaPorNumero;
            }
        }

        var command = new MovimentarContaCommand
        {
            IdentificacaoRequisicao = request.IdentificacaoRequisicao,
            IdContaCorrente = idContaParaMovimentar,
            NumeroContaDestino = request.NumeroContaDestino,
            Valor = request.Valor,
            TipoMovimento = request.TipoMovimento
        };

        var result = await _mediator.Send(command);

        if (result.Sucesso)
        {
            return NoContent();
        }

        return BadRequest(result.Erro);
    }

    /// <summary>
    /// Consulta o saldo da conta corrente
    /// </summary>
    /// <returns>Saldo da conta</returns>
    [HttpGet("saldo")]
    [Authorize]
    [ProducesResponseType(typeof(SaldoResponse), 200)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ConsultarSaldo()
    {
        var idContaCorrente = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idContaCorrente))
        {
            return Forbid();
        }

        var query = new ConsultarSaldoQuery
        {
            IdContaCorrente = idContaCorrente
        };

        var result = await _mediator.Send(query);

        if (result.Sucesso)
        {
            return Ok(result.Dados);
        }

        return BadRequest(result.Erro);
    }

    /// <summary>
    /// Valida se uma conta existe e está ativa (endpoint interno)
    /// </summary>
    /// <param name="numeroConta">Número da conta</param>
    /// <returns>Status da validação</returns>
    [HttpGet("validar/{numeroConta}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    public async Task<IActionResult> ValidarConta(int numeroConta)
    {
        var query = new ConsultarSaldoQuery
        {
            IdContaCorrente = (await ObterIdContaPorNumero(numeroConta)) ?? ""
        };

        if (string.IsNullOrEmpty(query.IdContaCorrente))
        {
            return BadRequest(new ErroResponse("Conta não encontrada", "INVALID_ACCOUNT"));
        }

        var result = await _mediator.Send(query);

        if (result.Sucesso)
        {
            return Ok();
        }

        return BadRequest(result.Erro);
    }

    /// <summary>
    /// Obtém o ID da conta corrente por número (endpoint interno)
    /// </summary>
    /// <param name="numeroConta">Número da conta</param>
    /// <returns>ID da conta corrente</returns>
    [HttpGet("obter-id/{numeroConta}")]
    [Authorize]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    public async Task<IActionResult> ObterIdPorNumero(int numeroConta)
    {
        var idConta = await ObterIdContaPorNumero(numeroConta);
        if (string.IsNullOrEmpty(idConta))
        {
            return BadRequest(new ErroResponse("Conta não encontrada", "INVALID_ACCOUNT"));
        }

        return Ok(new { IdContaCorrente = idConta });
    }

    private async Task<string?> ObterIdContaPorNumero(int numeroConta)
    {
        var conta = await _contaRepository.ObterPorNumeroContaAsync(numeroConta);
        return conta?.IdContaCorrente;
    }
}

// DTOs para as requisições
public class CadastrarContaCorrenteRequest
{
    public string Cpf { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string NomeTitular { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Identificacao { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class InativarContaRequest
{
    public string Senha { get; set; } = string.Empty;
}

public class MovimentarContaRequest
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public int? NumeroConta { get; set; }
    public int? NumeroContaDestino { get; set; }
    public decimal Valor { get; set; }
    public string TipoMovimento { get; set; } = string.Empty;
}
