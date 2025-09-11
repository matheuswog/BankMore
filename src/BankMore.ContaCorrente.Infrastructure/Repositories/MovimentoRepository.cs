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
            INSERT INTO Movimento (IdentificacaoRequisicao, ContaCorrenteId, TipoMovimento, Valor, DataMovimento, Descricao)
            VALUES (@IdentificacaoRequisicao, @ContaCorrenteId, @TipoMovimento, @Valor, @DataMovimento, @Descricao);
            SELECT last_insert_rowid();";

        var id = await _context.Connection.QuerySingleAsync<int>(sql, movimento);
        movimento.Id = id;
        return movimento;
    }

    public async Task<decimal> CalcularSaldoAsync(int contaCorrenteId)
    {
        const string sql = @"
            SELECT 
                COALESCE(SUM(CASE WHEN TipoMovimento = 'C' THEN Valor ELSE 0 END), 0) -
                COALESCE(SUM(CASE WHEN TipoMovimento = 'D' THEN Valor ELSE 0 END), 0) as Saldo
            FROM Movimento 
            WHERE ContaCorrenteId = @ContaCorrenteId";

        return await _context.Connection.QuerySingleAsync<decimal>(sql, new { ContaCorrenteId = contaCorrenteId });
    }

    public async Task<bool> ExisteIdentificacaoRequisicaoAsync(string identificacaoRequisicao)
    {
        const string sql = "SELECT COUNT(1) FROM Movimento WHERE IdentificacaoRequisicao = @IdentificacaoRequisicao";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { IdentificacaoRequisicao = identificacaoRequisicao });
        return count > 0;
    }

    public async Task<List<Movimento>> ObterMovimentosPorContaAsync(int contaCorrenteId)
    {
        const string sql = @"
            SELECT Id, IdentificacaoRequisicao, ContaCorrenteId, TipoMovimento, Valor, DataMovimento, Descricao
            FROM Movimento 
            WHERE ContaCorrenteId = @ContaCorrenteId
            ORDER BY DataMovimento DESC";

        var movimentos = await _context.Connection.QueryAsync<Movimento>(sql, new { ContaCorrenteId = contaCorrenteId });
        return movimentos.ToList();
    }
}
