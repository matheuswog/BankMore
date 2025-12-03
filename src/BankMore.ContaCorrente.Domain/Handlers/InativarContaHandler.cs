using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class InativarContaHandler : IRequestHandler<InativarContaCommand, Result<bool>>
{
    private readonly IContaCorrenteRepository _repository;

    public InativarContaHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(InativarContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await _repository.ObterPorIdContaCorrenteAsync(request.IdContaCorrente);

        if (conta == null)
        {
            return Result<bool>.ErroResultado("Conta não encontrada", TipoFalha.InvalidAccount.ToString());
        }

        var senhaCriptografada = CriptografarSenha(request.Senha, conta.Salt);
        if (conta.Senha != senhaCriptografada)
        {
            return Result<bool>.ErroResultado("Senha inválida", TipoFalha.UserUnauthorized.ToString());
        }

        conta.Ativo = 0;
        await _repository.AtualizarAsync(conta);

        return Result<bool>.SucessoResultado(true);
    }

    private static string CriptografarSenha(string senha, string salt)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha + salt);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
