using Dapper;
using BankMore.Transferencia.Infrastructure.Data;
using BankMore.Transferencia.Domain.Interfaces;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public class IdempotenciaRepository : IIdempotenciaRepository
{
    private readonly DatabaseContext _context;

    public IdempotenciaRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteChaveAsync(string chaveIdempotencia)
    {
        const string sql = "SELECT COUNT(1) FROM idempotencia WHERE chave_idempotencia = @ChaveIdempotencia";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { ChaveIdempotencia = chaveIdempotencia });
        return count > 0;
    }

    public async Task SalvarAsync(string chaveIdempotencia, string requisicao, string resultado)
    {
        const string sql = @"
            INSERT OR REPLACE INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@ChaveIdempotencia, @Requisicao, @Resultado)";

        await _context.Connection.ExecuteAsync(sql, new { ChaveIdempotencia = chaveIdempotencia, Requisicao = requisicao, Resultado = resultado });
    }
}

