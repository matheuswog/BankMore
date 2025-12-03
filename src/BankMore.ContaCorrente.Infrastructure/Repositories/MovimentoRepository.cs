using Dapper;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Infrastructure.Data;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public class MovimentoRepository : IMovimentoRepository
{
    private readonly DatabaseContext _context;

    public MovimentoRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Movimento> InserirAsync(Movimento movimento)
    {
        const string sql = @"
            INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
            VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";

        await _context.Connection.ExecuteAsync(sql, movimento);
        return movimento;
    }

    public async Task<decimal> CalcularSaldoAsync(string idContaCorrente)
    {
        const string sql = @"
            SELECT 
                COALESCE(SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END), 0) -
                COALESCE(SUM(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END), 0) as Saldo
            FROM movimento 
            WHERE idcontacorrente = @IdContaCorrente";

        return await _context.Connection.QuerySingleAsync<decimal>(sql, new { IdContaCorrente = idContaCorrente });
    }


    public async Task<List<Movimento>> ObterMovimentosPorContaAsync(string idContaCorrente)
    {
        const string sql = @"
            SELECT idmovimento AS IdMovimento, idcontacorrente AS IdContaCorrente, 
                   datamovimento AS DataMovimento, tipomovimento AS TipoMovimento, 
                   valor AS Valor
            FROM movimento 
            WHERE idcontacorrente = @IdContaCorrente
            ORDER BY datamovimento DESC";

        var movimentos = await _context.Connection.QueryAsync<Movimento>(sql, new { IdContaCorrente = idContaCorrente });
        return movimentos.ToList();
    }
}
