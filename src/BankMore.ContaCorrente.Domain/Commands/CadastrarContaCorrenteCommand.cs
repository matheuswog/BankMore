using MediatR;
using BankMore.ContaCorrente.Domain.ValueObjects;

namespace BankMore.ContaCorrente.Domain.Commands;

public class CadastrarContaCorrenteCommand : IRequest<Result<string>>
{
    public string Cpf { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string NomeTitular { get; set; } = string.Empty;
}

public class Result<T>
{
    public bool Sucesso { get; set; }
    public T? Dados { get; set; }
    public ErroResponse? Erro { get; set; }
    
    public static Result<T> SucessoResultado(T dados) => new() { Sucesso = true, Dados = dados };
    public static Result<T> ErroResultado(string mensagem, string tipoFalha) => new() { Sucesso = false, Erro = new ErroResponse(mensagem, tipoFalha) };
}
