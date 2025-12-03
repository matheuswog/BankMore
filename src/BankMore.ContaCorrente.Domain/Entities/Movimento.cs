using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities;

public class Movimento
{
    [Required]
    [StringLength(37)]
    public string IdMovimento { get; set; } = string.Empty;
    
    [Required]
    [StringLength(37)]
    public string IdContaCorrente { get; set; } = string.Empty;
    
    [Required]
    [StringLength(25)]
    public string DataMovimento { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1)]
    public string TipoMovimento { get; set; } = string.Empty;
    
    [Required]
    public decimal Valor { get; set; }
}
