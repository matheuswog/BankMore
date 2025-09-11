using Dapper;
using Microsoft.Data.Sqlite;
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

    public async Task<ContaCorrente?> ObterPorIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Cpf, NomeTitular, NumeroConta, Senha, Ativo, DataCriacao, DataInativacao
            FROM ContaCorrente 
            WHERE Id = @Id";

        return await _context.Connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { Id = id });
    }

    public async Task<ContaCorrente?> ObterPorCpfAsync(string cpf)
    {
        const string sql = @"
            SELECT Id, Cpf, NomeTitular, NumeroConta, Senha, Ativo, DataCriacao, DataInativacao
            FROM ContaCorrente 
            WHERE Cpf = @Cpf";

        return await _context.Connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { Cpf = cpf });
    }

    public async Task<ContaCorrente?> ObterPorNumeroContaAsync(string numeroConta)
    {
        const string sql = @"
            SELECT Id, Cpf, NomeTitular, NumeroConta, Senha, Ativo, DataCriacao, DataInativacao
            FROM ContaCorrente 
            WHERE NumeroConta = @NumeroConta";

        return await _context.Connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { NumeroConta = numeroConta });
    }

    public async Task<ContaCorrente?> ObterPorCpfOuNumeroContaAsync(string identificacao)
    {
        const string sql = @"
            SELECT Id, Cpf, NomeTitular, NumeroConta, Senha, Ativo, DataCriacao, DataInativacao
            FROM ContaCorrente 
            WHERE Cpf = @Identificacao OR NumeroConta = @Identificacao";

        return await _context.Connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { Identificacao = identificacao });
    }

    public async Task<ContaCorrente> InserirAsync(ContaCorrente contaCorrente)
    {
        const string sql = @"
            INSERT INTO ContaCorrente (Cpf, NomeTitular, NumeroConta, Senha, Ativo, DataCriacao, DataInativacao)
            VALUES (@Cpf, @NomeTitular, @NumeroConta, @Senha, @Ativo, @DataCriacao, @DataInativacao);
            SELECT last_insert_rowid();";

        var id = await _context.Connection.QuerySingleAsync<int>(sql, contaCorrente);
        contaCorrente.Id = id;
        return contaCorrente;
    }

    public async Task AtualizarAsync(ContaCorrente contaCorrente)
    {
        const string sql = @"
            UPDATE ContaCorrente 
            SET Cpf = @Cpf, NomeTitular = @NomeTitular, NumeroConta = @NumeroConta, 
                Senha = @Senha, Ativo = @Ativo, DataCriacao = @DataCriacao, DataInativacao = @DataInativacao
            WHERE Id = @Id";

        await _context.Connection.ExecuteAsync(sql, contaCorrente);
    }

    public async Task<bool> ExisteCpfAsync(string cpf)
    {
        const string sql = "SELECT COUNT(1) FROM ContaCorrente WHERE Cpf = @Cpf";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { Cpf = cpf });
        return count > 0;
    }

    public async Task<bool> ExisteNumeroContaAsync(string numeroConta)
    {
        const string sql = "SELECT COUNT(1) FROM ContaCorrente WHERE NumeroConta = @NumeroConta";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { NumeroConta = numeroConta });
        return count > 0;
    }
}
