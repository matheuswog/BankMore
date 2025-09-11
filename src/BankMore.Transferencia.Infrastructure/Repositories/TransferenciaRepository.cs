using Dapper;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Infrastructure.Data;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly DatabaseContext _context;

    public TransferenciaRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Transferencia> InserirAsync(Domain.Entities.Transferencia transferencia)
    {
        const string sql = @"
            INSERT INTO Transferencia (IdentificacaoRequisicao, ContaOrigemId, ContaDestinoId, Valor, DataTransferencia, Descricao, Processada)
            VALUES (@IdentificacaoRequisicao, @ContaOrigemId, @ContaDestinoId, @Valor, @DataTransferencia, @Descricao, @Processada);
            SELECT last_insert_rowid();";

        var id = await _context.Connection.QuerySingleAsync<int>(sql, transferencia);
        transferencia.Id = id;
        return transferencia;
    }

    public async Task<bool> ExisteIdentificacaoRequisicaoAsync(string identificacaoRequisicao)
    {
        const string sql = "SELECT COUNT(1) FROM Transferencia WHERE IdentificacaoRequisicao = @IdentificacaoRequisicao";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { IdentificacaoRequisicao = identificacaoRequisicao });
        return count > 0;
    }

    public async Task<Domain.Entities.Transferencia?> ObterPorIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, IdentificacaoRequisicao, ContaOrigemId, ContaDestinoId, Valor, DataTransferencia, Descricao, Processada
            FROM Transferencia 
            WHERE Id = @Id";

        return await _context.Connection.QueryFirstOrDefaultAsync<Domain.Entities.Transferencia>(sql, new { Id = id });
    }
}
