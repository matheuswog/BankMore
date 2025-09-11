namespace BankMore.ContaCorrente.Domain.ValueObjects;

public class ErroResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public string TipoFalha { get; set; } = string.Empty;
    
    public ErroResponse(string mensagem, string tipoFalha)
    {
        Mensagem = mensagem;
        TipoFalha = tipoFalha;
    }
}
