using Dapper;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Infrastructure.Data;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly DatabaseContext _context;

    public ContaCorrenteRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.ContaCorrente?> ObterPorIdContaCorrenteAsync(string idContaCorrente)
    {
        const string sql = @"
            SELECT idcontacorrente AS IdContaCorrente, numero AS Numero, nome AS Nome, cpf AS Cpf,
                   ativo AS Ativo, senha AS Senha, salt AS Salt
            FROM contacorrente 
            WHERE idcontacorrente = @IdContaCorrente";

        return await _context.Connection.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(
            sql, new { IdContaCorrente = idContaCorrente });
    }

    public async Task<Domain.Entities.ContaCorrente?> ObterPorNumeroContaAsync(int numero)
    {
        const string sql = @"
            SELECT idcontacorrente AS IdContaCorrente, numero AS Numero, nome AS Nome, cpf AS Cpf,
                   ativo AS Ativo, senha AS Senha, salt AS Salt
            FROM contacorrente 
            WHERE numero = @Numero";

        return await _context.Connection.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(
            sql, new { Numero = numero });
    }

    public async Task<Domain.Entities.ContaCorrente?> ObterPorCpfOuNumeroContaAsync(string identificacao)
    {
        if (int.TryParse(identificacao, out var numero))
        {
            return await ObterPorNumeroContaAsync(numero);
        }
        
        const string sql = @"
            SELECT idcontacorrente AS IdContaCorrente, numero AS Numero, nome AS Nome, cpf AS Cpf,
                   ativo AS Ativo, senha AS Senha, salt AS Salt
            FROM contacorrente 
            WHERE cpf = @Identificacao";

        return await _context.Connection.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(
            sql, new { Identificacao = identificacao });
    }

    public async Task<Domain.Entities.ContaCorrente> InserirAsync(Domain.Entities.ContaCorrente contaCorrente)
    {
        const string sql = @"
            INSERT INTO contacorrente (idcontacorrente, numero, nome, cpf, ativo, senha, salt)
            VALUES (@IdContaCorrente, @Numero, @Nome, @Cpf, @Ativo, @Senha, @Salt)";

        await _context.Connection.ExecuteAsync(sql, contaCorrente);
        return contaCorrente;
    }

    public async Task AtualizarAsync(Domain.Entities.ContaCorrente contaCorrente)
    {
        const string sql = @"
            UPDATE contacorrente 
            SET numero = @Numero, nome = @Nome, cpf = @Cpf, ativo = @Ativo, senha = @Senha, salt = @Salt
            WHERE idcontacorrente = @IdContaCorrente";

        await _context.Connection.ExecuteAsync(sql, contaCorrente);
    }

    public async Task<bool> ExisteCpfAsync(string cpf)
    {
        const string sql = "SELECT COUNT(1) FROM contacorrente WHERE cpf = @Cpf";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { Cpf = cpf });
        return count > 0;
    }

    public async Task<bool> ExisteNumeroContaAsync(int numero)
    {
        const string sql = "SELECT COUNT(1) FROM contacorrente WHERE numero = @Numero";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { Numero = numero });
        return count > 0;
    }
}
