using System.ComponentModel.DataAnnotations;

namespace BankMore.Transferencia.Domain.Entities;

public class Transferencia
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    
    public int ContaOrigemId { get; set; }
    
    public int ContaDestinoId { get; set; }
    
    [Required]
    public decimal Valor { get; set; }
    
    public DateTime DataTransferencia { get; set; } = DateTime.UtcNow;
    
    [StringLength(500)]
    public string? Descricao { get; set; }
    
    public bool Processada { get; set; } = false;
}
