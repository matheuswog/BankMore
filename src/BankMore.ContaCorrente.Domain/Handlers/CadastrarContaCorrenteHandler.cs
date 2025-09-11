using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Infrastructure.Repositories;
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
        // Validar CPF
        if (!ValidarCpf(request.Cpf))
        {
            return Result<string>.ErroResultado("CPF inválido", TipoFalha.InvalidDocument.ToString());
        }

        // Verificar se CPF já existe
        if (await _repository.ExisteCpfAsync(request.Cpf))
        {
            return Result<string>.ErroResultado("CPF já cadastrado", TipoFalha.InvalidDocument.ToString());
        }

        // Gerar número da conta
        var numeroConta = GerarNumeroConta();

        // Verificar se número da conta já existe
        while (await _repository.ExisteNumeroContaAsync(numeroConta))
        {
            numeroConta = GerarNumeroConta();
        }

        // Criptografar senha
        var senhaCriptografada = CriptografarSenha(request.Senha);

        // Criar conta corrente
        var contaCorrente = new ContaCorrente
        {
            Cpf = request.Cpf,
            NomeTitular = request.NomeTitular,
            NumeroConta = numeroConta,
            Senha = senhaCriptografada,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        await _repository.InserirAsync(contaCorrente);

        return Result<string>.SucessoResultado(numeroConta);
    }

    private static bool ValidarCpf(string cpf)
    {
        cpf = cpf.Replace(".", "").Replace("-", "").Trim();

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

    private static string GerarNumeroConta()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static string CriptografarSenha(string senha)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
