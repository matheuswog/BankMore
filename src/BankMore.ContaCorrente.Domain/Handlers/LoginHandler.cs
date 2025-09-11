using MediatR;
using BankMore.ContaCorrente.Domain.Commands;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BankMore.ContaCorrente.Domain.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IConfiguration _configuration;

    public LoginHandler(IContaCorrenteRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar conta por CPF ou número da conta
        var conta = await _repository.ObterPorCpfOuNumeroContaAsync(request.Identificacao);

        if (conta == null)
        {
            return Result<LoginResponse>.ErroResultado("Conta não encontrada", TipoFalha.UserUnauthorized.ToString());
        }

        // Verificar se a conta está ativa
        if (!conta.Ativo)
        {
            return Result<LoginResponse>.ErroResultado("Conta inativa", TipoFalha.UserUnauthorized.ToString());
        }

        // Verificar senha
        var senhaCriptografada = CriptografarSenha(request.Senha);
        if (conta.Senha != senhaCriptografada)
        {
            return Result<LoginResponse>.ErroResultado("Senha inválida", TipoFalha.UserUnauthorized.ToString());
        }

        // Gerar token JWT
        var token = GerarToken(conta);

        var response = new LoginResponse
        {
            Token = token,
            ContaCorrenteId = conta.Id,
            NumeroConta = conta.NumeroConta
        };

        return Result<LoginResponse>.SucessoResultado(response);
    }

    private static string CriptografarSenha(string senha)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GerarToken(Domain.Entities.ContaCorrente conta)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, conta.Id.ToString()),
            new Claim(ClaimTypes.Name, conta.NumeroConta),
            new Claim("Cpf", conta.Cpf),
            new Claim("NomeTitular", conta.NomeTitular)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
