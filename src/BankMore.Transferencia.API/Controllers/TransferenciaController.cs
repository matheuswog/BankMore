using Microsoft.AspNetCore.Mvc;
using MediatR;
using BankMore.Transferencia.Domain.Commands;
using BankMore.Transferencia.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BankMore.Transferencia.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferenciaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferenciaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Efetua transferência entre contas da mesma instituição
    /// </summary>
    /// <param name="request">Dados da transferência</param>
    /// <returns>Confirmação da transferência</returns>
    [HttpPost("efetuar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> EfetuarTransferencia([FromBody] EfetuarTransferenciaRequest request)
    {
        var idContaCorrenteOrigem = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idContaCorrenteOrigem))
        {
            return Forbid();
        }

        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        var command = new EfetuarTransferenciaCommand
        {
            IdentificacaoRequisicao = request.IdentificacaoRequisicao,
            IdContaCorrenteOrigem = idContaCorrenteOrigem,
            NumeroContaDestino = request.NumeroContaDestino,
            Valor = request.Valor,
            TokenJwt = token
        };

        var result = await _mediator.Send(command);

        if (result.Sucesso)
        {
            return NoContent();
        }

        return BadRequest(result.Erro);
    }
}

// DTOs para as requisições
public class EfetuarTransferenciaRequest
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public int NumeroContaDestino { get; set; }
    public decimal Valor { get; set; }
}
