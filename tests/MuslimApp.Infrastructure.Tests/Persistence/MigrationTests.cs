using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging.Abstractions;
using MuslimApp.Infrastructure.Persistence;

namespace MuslimApp.Infrastructure.Tests.Persistence;

public class MigrationTests
{
    /// <summary>Every table named in the data design must exist after migrating.</summary>
    private static readonly string[] ExpectedTables =
    [
        "AppSettings",
        "PrayerSettings",
        "PrayerTimes",
        "QuranSurahs",
        "QuranAyahs",
        "QuranBookmarks",
        "QuranReadingProgress",
        "AzkarCategories",
        "AzkarItems",
        "AzkarSources",
        "DhikrProgress",
        "DuaCategories",
        "Duas",
        "DuaSources",
        "TasbihPresets",
        "TasbihSessions",
        "NotificationSettings"
    ];

    /// <summary>The indexes that exist because a query in the app would otherwise scan a large table.</summary>
    private static readonly string[] ExpectedIndexes =
    [
        "IX_QuranAyahs_SurahId_AyahNumber",
        "IX_QuranAyahs_Juz",
        "IX_QuranAyahs_PageNumber",
        "IX_AzkarItems_CategoryId_Order",
        "IX_Duas_CategoryId_Order",
        "IX_PrayerTimes_Date",
        "IX_DhikrProgress_SessionDate"
    ];

    [Fact]
    public void Migrating_creates_every_expected_table()
    {
        using var database = new SqliteTestDatabase();
        using MuslimDbContext context = database.CreateDbContext();

        List<string> tables = QueryNames(context, "SELECT name FROM sqlite_master WHERE type = 'table'");

        tables.Should().Contain(ExpectedTables);
    }

    [Fact]
    public void Migrating_creates_every_expected_index()
    {
        using var database = new SqliteTestDatabase();
        using MuslimDbContext context = database.CreateDbContext();

        List<string> indexes = QueryNames(context, "SELECT name FROM sqlite_master WHERE type = 'index'");

        indexes.Should().Contain(ExpectedIndexes);
    }

    [Fact]
    public void Migrating_leaves_no_pending_migrations()
    {
        using var database = new SqliteTestDatabase();
        using MuslimDbContext context = database.CreateDbContext();

        context.Database.GetPendingMigrations().Should().BeEmpty();
    }

    [Fact]
    public void The_migration_snapshot_matches_the_model()
    {
        // A model change without a matching migration would only surface as a runtime failure on a user's
        // device, so it is caught here instead.
        using var database = new SqliteTestDatabase();
        using MuslimDbContext context = database.CreateDbContext();

        IModel? snapshot = context.GetService<IMigrationsAssembly>().ModelSnapshot?.Model;
        snapshot.Should().NotBeNull("the migrations assembly must contain a model snapshot");

        IModel finalizedSnapshot = context.GetService<IModelRuntimeInitializer>()
            .Initialize(((IMutableModel)snapshot!).FinalizeModel(), designTime: true, validationLogger: null);

        IReadOnlyList<MigrationOperation> differences = context.GetService<IMigrationsModelDiffer>()
            .GetDifferences(
                finalizedSnapshot.GetRelationalModel(),
                context.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        differences.Should().BeEmpty("the snapshot is out of date; add a migration");
    }

    [Fact]
    public async Task The_initializer_reports_success_on_a_fresh_database()
    {
        using var database = new SqliteTestDatabase();
        var initializer = new DatabaseInitializer(database, NullLogger<DatabaseInitializer>.Instance);

        bool prepared = await initializer.InitializeAsync();

        prepared.Should().BeTrue();
    }

    [Fact]
    public async Task The_initializer_is_safe_to_run_twice()
    {
        using var database = new SqliteTestDatabase();
        var initializer = new DatabaseInitializer(database, NullLogger<DatabaseInitializer>.Instance);

        await initializer.InitializeAsync();
        bool second = await initializer.InitializeAsync();

        second.Should().BeTrue();
    }

    [Fact]
    public async Task The_initializer_reports_failure_instead_of_throwing_when_the_database_is_unusable()
    {
        // A path that cannot be opened stands in for a corrupt or unwritable database file.
        DbContextOptions<MuslimDbContext> options = new DbContextOptionsBuilder<MuslimDbContext>()
            .UseSqlite("Data Source=/this/path/does/not/exist/muslimapp.db")
            .Options;

        var initializer = new DatabaseInitializer(
            new StaticContextFactory(options),
            NullLogger<DatabaseInitializer>.Instance);

        bool prepared = await initializer.InitializeAsync();

        prepared.Should().BeFalse();
    }

    private static List<string> QueryNames(MuslimDbContext context, string sql)
    {
        var names = new List<string>();

        context.Database.OpenConnection();

        using SqliteCommand command = (SqliteCommand)context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private sealed class StaticContextFactory(DbContextOptions<MuslimDbContext> options) : IDbContextFactory<MuslimDbContext>
    {
        public MuslimDbContext CreateDbContext() => new(options);
    }
}
