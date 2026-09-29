using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// The single-row settings tables. Each getter creates the row on first use, so no caller ever has to
/// deal with missing configuration.
/// </summary>
public sealed class SettingsRepository(IDbContextFactory<MuslimDbContext> contextFactory) : ISettingsRepository
{
    public async Task<AppSettings> GetAppSettingsAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        AppSettings? settings = await context.AppSettings
            .FirstOrDefaultAsync(x => x.Id == AppSettings.SingletonId, cancellationToken)
            .ConfigureAwait(false);

        if (settings is not null)
        {
            return settings;
        }

        settings = new AppSettings { Id = AppSettings.SingletonId };
        context.AppSettings.Add(settings);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return settings;
    }

    public async Task SaveAppSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Id = AppSettings.SingletonId;

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await UpsertAsync(context, context.AppSettings, settings, AppSettings.SingletonId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PrayerSettings> GetPrayerSettingsAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        PrayerSettings? settings = await context.PrayerSettings
            .FirstOrDefaultAsync(x => x.Id == PrayerSettings.SingletonId, cancellationToken)
            .ConfigureAwait(false);

        if (settings is not null)
        {
            return settings;
        }

        // The device zone is the only sensible starting point, and the engine requires one. The location
        // itself stays unset until the user grants a fix or picks a city.
        settings = new PrayerSettings
        {
            Id = PrayerSettings.SingletonId,
            TimeZoneId = TimeZoneInfo.Local.Id,
            HasLocation = false
        };

        context.PrayerSettings.Add(settings);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return settings;
    }

    public async Task SavePrayerSettingsAsync(PrayerSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Id = PrayerSettings.SingletonId;

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await UpsertAsync(context, context.PrayerSettings, settings, PrayerSettings.SingletonId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<NotificationSettings> GetNotificationSettingsAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        NotificationSettings? settings = await context.NotificationSettings
            .FirstOrDefaultAsync(x => x.Id == NotificationSettings.SingletonId, cancellationToken)
            .ConfigureAwait(false);

        if (settings is not null)
        {
            return settings;
        }

        settings = new NotificationSettings { Id = NotificationSettings.SingletonId };
        context.NotificationSettings.Add(settings);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return settings;
    }

    public async Task SaveNotificationSettingsAsync(NotificationSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Id = NotificationSettings.SingletonId;

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await UpsertAsync(context, context.NotificationSettings, settings, NotificationSettings.SingletonId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Entities come back detached because every call uses its own context, so an update has to be
    /// expressed as "replace the row with this id".
    /// </summary>
    private static async Task UpsertAsync<TEntity>(
        MuslimDbContext context,
        DbSet<TEntity> set,
        TEntity entity,
        int id,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        TEntity? existing = await set.FindAsync([id], cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            set.Add(entity);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(entity);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
