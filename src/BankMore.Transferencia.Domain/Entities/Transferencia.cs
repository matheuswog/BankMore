using System.ComponentModel.DataAnnotations;

namespace BankMore.Transferencia.Domain.Entities;

public class Transferencia
{
    [Required]
    [StringLength(37)]
    public string IdTransferencia { get; set; } = string.Empty;
    
    [Required]
    [StringLength(37)]
    public string IdContaCorrenteOrigem { get; set; } = string.Empty;
    
    [Required]
    [StringLength(37)]
    public string IdContaCorrenteDestino { get; set; } = string.Empty;
    
    [Required]
    [StringLength(25)]
    public string DataMovimento { get; set; } = string.Empty;
    
    [Required]
    public decimal Valor { get; set; }
}
