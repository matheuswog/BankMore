using System.ComponentModel.DataAnnotations;

namespace BankMore.Transferencia.Domain.ValueObjects;

public class TransferenciaRequest
{
    [Required]
    [StringLength(50)]
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    
    [Required]
    public int ContaDestinoId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Valor { get; set; }
}
