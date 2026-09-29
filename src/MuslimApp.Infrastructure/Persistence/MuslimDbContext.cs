using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Entities.Azkar;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Quran;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Entities.Tasbih;
using MuslimApp.Infrastructure.Persistence.Converters;
using DuaEntities = MuslimApp.Domain.Entities.Dua;

namespace MuslimApp.Infrastructure.Persistence;

/// <summary>
/// The single local database. Everything the app needs to function lives here, which is what makes the
/// product genuinely offline-first rather than offline-tolerant.
/// </summary>
public class MuslimDbContext(DbContextOptions<MuslimDbContext> options) : DbContext(options)
{
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();

    public DbSet<PrayerSettings> PrayerSettings => Set<PrayerSettings>();

    public DbSet<PrayerTime> PrayerTimes => Set<PrayerTime>();

    public DbSet<QuranSurah> QuranSurahs => Set<QuranSurah>();

    public DbSet<QuranAyah> QuranAyahs => Set<QuranAyah>();

    public DbSet<QuranBookmark> QuranBookmarks => Set<QuranBookmark>();

    public DbSet<QuranReadingProgress> QuranReadingProgress => Set<QuranReadingProgress>();

    public DbSet<AzkarCategory> AzkarCategories => Set<AzkarCategory>();

    public DbSet<AzkarItem> AzkarItems => Set<AzkarItem>();

    public DbSet<AzkarSource> AzkarSources => Set<AzkarSource>();

    public DbSet<DhikrProgress> DhikrProgress => Set<DhikrProgress>();

    public DbSet<DuaEntities.DuaCategory> DuaCategories => Set<DuaEntities.DuaCategory>();

    public DbSet<DuaEntities.Dua> Duas => Set<DuaEntities.Dua>();

    public DbSet<DuaEntities.DuaSource> DuaSources => Set<DuaEntities.DuaSource>();

    public DbSet<TasbihPreset> TasbihPresets => Set<TasbihPreset>();

    public DbSet<TasbihSession> TasbihSessions => Set<TasbihSession>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        // Applied model-wide so no individual entity can forget it and end up with a column SQLite cannot
        // order or compare.
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToUtcTicksConverter>();

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MuslimDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
