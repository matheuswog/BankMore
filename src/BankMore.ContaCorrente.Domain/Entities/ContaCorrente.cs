using System.ComponentModel.DataAnnotations;

namespace BankMore.ContaCorrente.Domain.Entities;

public class ContaCorrente
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(11)]
    public string Cpf { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string NomeTitular { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string NumeroConta { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Senha { get; set; } = string.Empty;
    
    public bool Ativo { get; set; } = true;
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataInativacao { get; set; }
}
