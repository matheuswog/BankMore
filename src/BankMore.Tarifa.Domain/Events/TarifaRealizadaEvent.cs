namespace BankMore.Tarifa.Domain.Events;

public class TarifaRealizadaEvent
{
    public string IdTarifa { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public decimal ValorTarifado { get; set; }
    public string IdentificacaoRequisicaoTransferencia { get; set; } = string.Empty;
}

