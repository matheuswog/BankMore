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
        var conta = await _repository.ObterPorCpfOuNumeroContaAsync(request.Identificacao);

        if (conta == null)
        {
            return Result<LoginResponse>.ErroResultado("Conta não encontrada", TipoFalha.UserUnauthorized.ToString());
        }

        if (conta.Ativo != 1)
        {
            return Result<LoginResponse>.ErroResultado("Conta inativa", TipoFalha.UserUnauthorized.ToString());
        }

        var senhaCriptografada = CriptografarSenha(request.Senha, conta.Salt);
        if (conta.Senha != senhaCriptografada)
        {
            return Result<LoginResponse>.ErroResultado("Senha inválida", TipoFalha.UserUnauthorized.ToString());
        }

        var token = GerarToken(conta);

        var response = new LoginResponse
        {
            Token = token,
            IdContaCorrente = conta.IdContaCorrente,
            NumeroConta = conta.Numero.ToString()
        };

        return Result<LoginResponse>.SucessoResultado(response);
    }

    private static string CriptografarSenha(string senha, string salt)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(senha + salt);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GerarToken(Domain.Entities.ContaCorrente conta)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, conta.IdContaCorrente),
            new Claim("NumeroConta", conta.Numero.ToString()),
            new Claim("Nome", conta.Nome)
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
