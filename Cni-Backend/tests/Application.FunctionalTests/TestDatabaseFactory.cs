namespace Backend.Application.FunctionalTests;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync()
    {
        // Use SQLite in-memory database for fast testing without requiring Docker or external database
        var database = new InMemoryTestDatabase();

        await database.InitialiseAsync();

        return database;
    }
}
