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

    public Task ResetAsync()
    {
        // For in-memory SQLite, data is isolated per connection
        // Each test can use a fresh state
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
