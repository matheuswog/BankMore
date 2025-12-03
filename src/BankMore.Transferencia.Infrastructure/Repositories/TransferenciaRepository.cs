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
            INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
            VALUES (@IdTransferencia, @IdContaCorrenteOrigem, @IdContaCorrenteDestino, @DataMovimento, @Valor)";

        await _context.Connection.ExecuteAsync(sql, transferencia);
        return transferencia;
    }


    public async Task<Domain.Entities.Transferencia?> ObterPorIdAsync(string idTransferencia)
    {
        const string sql = @"
            SELECT idtransferencia AS IdTransferencia, idcontacorrente_origem AS IdContaCorrenteOrigem, 
                   idcontacorrente_destino AS IdContaCorrenteDestino, datamovimento AS DataMovimento, valor AS Valor
            FROM transferencia 
            WHERE idtransferencia = @IdTransferencia";

        return await _context.Connection.QueryFirstOrDefaultAsync<Domain.Entities.Transferencia>(sql, new { IdTransferencia = idTransferencia });
    }
}
