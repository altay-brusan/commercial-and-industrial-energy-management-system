using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Backend.Application.FunctionalTests;

public class InMemoryTestDatabase : ITestDatabase
{
    private readonly string _connectionString;
    private SqliteConnection _connection = null!;

    public InMemoryTestDatabase()
    {
        _connectionString = "DataSource=:memory:";
    }

    public async Task InitialiseAsync()
    {
        _connection = new SqliteConnection(_connectionString);
        await _connection.OpenAsync();
        // Database schema will be created by the CustomWebApplicationFactory
        // when it configures the ApplicationDbContext with EnsureCreated
    }

    public DbConnection GetConnection()
    {
        return _connection;
    }

    public string GetConnectionString()
    {
        return _connectionString;
    }

    public async Task ResetAsync()
    {
        // All tests share the same in-memory SQLite connection, so we must
        // delete all rows between tests to avoid data leaking across tests.
        using var cmd = _connection.CreateCommand();

        // Get all user-created table names
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory'";
        var tables = new List<string>();
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }

        // Disable FK constraints, delete all data, re-enable
        cmd.CommandText = "PRAGMA foreign_keys = OFF";
        await cmd.ExecuteNonQueryAsync();

        foreach (var table in tables)
        {
            cmd.CommandText = $"DELETE FROM \"{table}\"";
            await cmd.ExecuteNonQueryAsync();
        }

        cmd.CommandText = "PRAGMA foreign_keys = ON";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
