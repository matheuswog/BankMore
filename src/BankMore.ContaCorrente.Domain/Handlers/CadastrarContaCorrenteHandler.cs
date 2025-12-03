using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class CadastrarContaCorrenteHandler : IRequestHandler<CadastrarContaCorrenteCommand, Result<string>>
{
    private readonly IContaCorrenteRepository _repository;

    public CadastrarContaCorrenteHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(CadastrarContaCorrenteCommand request, CancellationToken cancellationToken)
    {
        var cpfLimpo = request.Cpf.Replace(".", "").Replace("-", "").Trim();
        
        if (!ValidarCpf(cpfLimpo))
        {
            return Result<string>.ErroResultado("CPF inválido", TipoFalha.InvalidDocument.ToString());
        }

        if (await _repository.ExisteCpfAsync(cpfLimpo))
        {
            return Result<string>.ErroResultado("CPF já cadastrado", TipoFalha.InvalidDocument.ToString());
        }

        var random = new Random();
        int numeroConta;
        do
        {
            numeroConta = random.Next(100000, 999999);
        } while (await _repository.ExisteNumeroContaAsync(numeroConta));

        var idContaCorrente = Guid.NewGuid().ToString();

        var salt = GerarSalt();
        var senhaCriptografada = CriptografarSenha(request.Senha, salt);

        var contaCorrente = new Entities.ContaCorrente
        {
            IdContaCorrente = idContaCorrente,
            Numero = numeroConta,
            Nome = request.NomeTitular,
            Cpf = cpfLimpo,
            Senha = senhaCriptografada,
            Salt = salt,
            Ativo = 1
        };

        await _repository.InserirAsync(contaCorrente);

        return Result<string>.SucessoResultado(numeroConta.ToString());
    }

    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            return false;

        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        string digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();

        return cpf.EndsWith(digito);
    }

    private static string GerarSalt()
    {
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    private static string CriptografarSenha(string senha, string salt)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha + salt);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
