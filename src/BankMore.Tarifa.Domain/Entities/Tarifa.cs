using System.ComponentModel.DataAnnotations;

namespace BankMore.Tarifa.Domain.Entities;

public class Tarifa
{
    public int Id { get; set; }
    
    [Required]
    public int ContaCorrenteId { get; set; }
    
    [Required]
    public decimal ValorTarifado { get; set; }
    
    public DateTime DataTarifacao { get; set; } = DateTime.UtcNow;
    
    [StringLength(500)]
    public string? Descricao { get; set; }
    
    [Required]
    [StringLength(50)]
    public string IdentificacaoTransferencia { get; set; } = string.Empty;
}
