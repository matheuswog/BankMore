namespace BankMore.Tarifa.Domain.ValueObjects;

public class TarifaRealizadaEvent
{
    public int ContaCorrenteId { get; set; }
    public decimal ValorTarifado { get; set; }
    public DateTime DataTarifacao { get; set; }
}
