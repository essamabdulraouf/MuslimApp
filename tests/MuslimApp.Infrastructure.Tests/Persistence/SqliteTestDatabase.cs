using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MuslimApp.Infrastructure.Persistence;

namespace MuslimApp.Infrastructure.Tests.Persistence;

/// <summary>
/// A real SQLite file, created through the real migrations. An in-memory provider would hide exactly the
/// problems worth testing: SQLite type mapping, unique indexes and the migration scripts themselves.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable, IDbContextFactory<MuslimDbContext>
{
    private readonly DbContextOptions<MuslimDbContext> _options;

    public SqliteTestDatabase()
    {
        FilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"muslimapp-tests-{Guid.NewGuid():N}.db");

        _options = new DbContextOptionsBuilder<MuslimDbContext>()
            .UseSqlite($"Data Source={FilePath}")
            .Options;

        using MuslimDbContext context = CreateDbContext();
        context.Database.Migrate();
    }

    public string FilePath { get; }

    public MuslimDbContext CreateDbContext() => new(_options);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (!File.Exists(FilePath))
        {
            return;
        }

        try
        {
            File.Delete(FilePath);
        }
        catch (IOException)
        {
            // A leftover temporary file is not worth failing a test run over.
        }
    }
}
