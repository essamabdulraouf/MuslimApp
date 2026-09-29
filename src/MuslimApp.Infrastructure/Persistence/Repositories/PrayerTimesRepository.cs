using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cache of computed timetables. Every row is derived data, so clearing the table is always safe.
/// </summary>
public sealed class PrayerTimesRepository(IDbContextFactory<MuslimDbContext> contextFactory) : IPrayerTimesRepository
{
    public async Task<PrayerTime?> GetAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.PrayerTimes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Date == date, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PrayerTime>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.PrayerTimes
            .AsNoTracking()
            .Where(x => x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpsertAsync(PrayerTime prayerTime, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prayerTime);

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        PrayerTime? existing = await context.PrayerTimes
            .FirstOrDefaultAsync(x => x.Date == prayerTime.Date, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            prayerTime.Id = 0;
            context.PrayerTimes.Add(prayerTime);
        }
        else
        {
            existing.Fajr = prayerTime.Fajr;
            existing.Sunrise = prayerTime.Sunrise;
            existing.Dhuhr = prayerTime.Dhuhr;
            existing.Asr = prayerTime.Asr;
            existing.Maghrib = prayerTime.Maghrib;
            existing.Isha = prayerTime.Isha;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        await context.PrayerTimes.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
