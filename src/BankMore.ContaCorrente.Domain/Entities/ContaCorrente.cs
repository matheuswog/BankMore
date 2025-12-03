using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities;

public class ContaCorrente
{
    [Required]
    [StringLength(37)]
    public string IdContaCorrente { get; set; } = string.Empty;
    
    [Required]
    public int Numero { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    [StringLength(11)]
    public string? Cpf { get; set; }
    
    [Required]
    public int Ativo { get; set; } = 1;
    
    [Required]
    [StringLength(100)]
    public string Senha { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Salt { get; set; } = string.Empty;
}
