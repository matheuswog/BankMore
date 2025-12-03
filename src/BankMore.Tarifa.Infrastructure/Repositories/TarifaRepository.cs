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
            INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor)
            VALUES (@IdTarifa, @IdContaCorrente, @DataMovimento, @Valor)";

        await _context.Connection.ExecuteAsync(sql, tarifa);
        return tarifa;
    }
}
