using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities;

public class Movimento
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    
    public int ContaCorrenteId { get; set; }
    
    public ContaCorrente? ContaCorrente { get; set; }
    
    [Required]
    [StringLength(1)]
    public string TipoMovimento { get; set; } = string.Empty; // C = Crédito, D = Débito
    
    [Required]
    public decimal Valor { get; set; }
    
    public DateTime DataMovimento { get; set; } = DateTime.UtcNow;
    
    [StringLength(500)]
    public string? Descricao { get; set; }
}
