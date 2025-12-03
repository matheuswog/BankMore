namespace BankMore.Transferencia.Domain.Events;

public class TransferenciaRealizadaEvent
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public string IdContaCorrenteOrigem { get; set; } = string.Empty;
    public decimal ValorTransferencia { get; set; }
    public DateTime DataHora { get; set; }
}

