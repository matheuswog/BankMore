using MediatR;
using BankMore.Tarifa.Domain.ValueObjects;

namespace BankMore.Tarifa.Domain.Commands;

public class ProcessarTarifaCommand : IRequest<Result<bool>>
{
    public string IdentificacaoRequisicao { get; set; } = string.Empty;
    public string IdContaCorrente { get; set; } = string.Empty;
    public decimal ValorTransferencia { get; set; }
}

public class Result<T>
{
    public bool Sucesso { get; set; }
    public T? Dados { get; set; }
    public ErroResponse? Erro { get; set; }
    
    public static Result<T> SucessoResultado(T dados) => new() { Sucesso = true, Dados = dados };
    public static Result<T> ErroResultado(string mensagem, string tipoFalha) => new() { Sucesso = false, Erro = new ErroResponse(mensagem, tipoFalha) };
}

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
