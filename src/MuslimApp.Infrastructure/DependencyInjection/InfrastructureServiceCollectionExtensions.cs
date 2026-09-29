using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Domain.Interfaces;
using MuslimApp.Infrastructure.Persistence;
using MuslimApp.Infrastructure.Persistence.Repositories;
using MuslimApp.Infrastructure.Prayer;
using MuslimApp.Infrastructure.Qibla;
using MuslimApp.Infrastructure.Time;

namespace MuslimApp.Infrastructure.DependencyInjection;

public sealed class InfrastructureOptions
{
    /// <summary>
    /// Absolute path to the SQLite file. The host supplies it because only the host knows the platform's
    /// private data directory.
    /// </summary>
    public string DatabasePath { get; set; } = "MuslimApp.db";
}

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers persistence, the calculation engines and the clock. Contains no UI and no device code, so
    /// the same registration works for the mobile app, a test host, or the optional API.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new InfrastructureOptions();
        configure(options);

        // A factory rather than a scoped context: a mobile app has no request scope, and view models are
        // long-lived. Each repository call gets a short-lived context instead of sharing one.
        services.AddDbContextFactory<MuslimDbContext>(builder =>
            builder.UseSqlite($"Data Source={options.DatabasePath}"));

        services.TryAddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        services.TryAddSingleton<ISettingsRepository, SettingsRepository>();
        services.TryAddSingleton<IPrayerTimesRepository, PrayerTimesRepository>();
        services.TryAddSingleton<IQuranRepository, QuranRepository>();
        services.TryAddSingleton<IAzkarRepository, AzkarRepository>();
        services.TryAddSingleton<IDuaRepository, DuaRepository>();
        services.TryAddSingleton<ITasbihRepository, TasbihRepository>();

        services.TryAddSingleton<IPrayerCalculationService, PrayerCalculationEngine>();
        services.TryAddSingleton<IQiblaService, QiblaCalculationService>();
        services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
