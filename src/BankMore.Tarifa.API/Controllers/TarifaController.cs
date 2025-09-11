using Microsoft.AspNetCore.Mvc;
using MediatR;
using BankMore.Tarifa.Domain.Commands;
using Microsoft.AspNetCore.Authorization;

namespace BankMore.Tarifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TarifaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TarifaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Processa tarifa de transferência (endpoint interno)
    /// </summary>
    /// <param name="request">Dados da tarifa</param>
    /// <returns>Confirmação do processamento</returns>
    [HttpPost("processar")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErroResponse), 400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ProcessarTarifa([FromBody] ProcessarTarifaRequest request)
    {
        var command = new ProcessarTarifaCommand
        {
            IdentificacaoRequisicao = request.IdentificacaoRequisicao,
            ContaCorrenteId = request.ContaCorrenteId,
            ValorTransferencia = request.ValorTransferencia
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
public class ProcessarTarifaRequest
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public int ContaCorrenteId { get; set; }
    public decimal ValorTransferencia { get; set; }
}
