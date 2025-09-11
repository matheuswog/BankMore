using Dapper;
using BankMore.Tarifa.Domain.Entities;
using BankMore.Tarifa.Infrastructure.Data;

namespace BankMore.Tarifa.Infrastructure.Repositories;

public class TarifaRepository : ITarifaRepository
{
    private readonly DatabaseContext _context;

    public TarifaRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Tarifa> InserirAsync(Domain.Entities.Tarifa tarifa)
    {
        const string sql = @"
            INSERT INTO Tarifa (ContaCorrenteId, ValorTarifado, DataTarifacao, Descricao, IdentificacaoTransferencia)
            VALUES (@ContaCorrenteId, @ValorTarifado, @DataTarifacao, @Descricao, @IdentificacaoTransferencia);
            SELECT last_insert_rowid();";

        var id = await _context.Connection.QuerySingleAsync<int>(sql, tarifa);
        tarifa.Id = id;
        return tarifa;
    }

    public async Task<bool> ExisteIdentificacaoTransferenciaAsync(string identificacaoTransferencia)
    {
        const string sql = "SELECT COUNT(1) FROM Tarifa WHERE IdentificacaoTransferencia = @IdentificacaoTransferencia";
        var count = await _context.Connection.QuerySingleAsync<int>(sql, new { IdentificacaoTransferencia = identificacaoTransferencia });
        return count > 0;
    }
}
