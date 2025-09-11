using Microsoft.Data.Sqlite;
using System.Data;

namespace BankMore.ContaCorrente.Infrastructure.Data;

public class DatabaseContext : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
