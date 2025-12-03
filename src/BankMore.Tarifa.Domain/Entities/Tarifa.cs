using System.ComponentModel.DataAnnotations;

namespace BankMore.Tarifa.Domain.Entities;

public class Tarifa
{
    [Required]
    [StringLength(37)]
    public string IdTarifa { get; set; } = string.Empty;
    
    [Required]
    [StringLength(37)]
    public string IdContaCorrente { get; set; } = string.Empty;
    
    [Required]
    [StringLength(25)]
    public string DataMovimento { get; set; } = string.Empty;
    
    [Required]
    public decimal Valor { get; set; }
}
