namespace BankMore.ContaCorrente.Domain.ValueObjects;

public class SaldoResponse
{
    public string NumeroConta { get; set; } = string.Empty;
    public string NomeTitular { get; set; } = string.Empty;
    public DateTime DataHoraConsulta { get; set; }
    public decimal Saldo { get; set; }
    
    public SaldoResponse(string numeroConta, string nomeTitular, decimal saldo)
    {
        NumeroConta = numeroConta;
        NomeTitular = nomeTitular;
        DataHoraConsulta = DateTime.UtcNow;
        Saldo = saldo;
    }
}
