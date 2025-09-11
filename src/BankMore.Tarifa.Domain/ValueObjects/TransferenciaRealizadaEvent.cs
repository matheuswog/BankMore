namespace BankMore.Tarifa.Domain.ValueObjects;

public class TransferenciaRealizadaEvent
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public int ContaCorrenteId { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataTransferencia { get; set; }
}
