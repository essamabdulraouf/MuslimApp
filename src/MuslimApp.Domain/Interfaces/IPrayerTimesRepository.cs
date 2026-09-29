using MuslimApp.Domain.Entities.Prayer;

namespace MuslimApp.Domain.Interfaces;

/// <summary>
/// Cache for computed timetables. Everything here is derived data: deleting the table must never lose
/// user state, which is why <see cref="ClearAsync"/> is safe to call whenever settings change.
/// </summary>
public interface IPrayerTimesRepository
{
    Task<PrayerTime?> GetAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrayerTime>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task UpsertAsync(PrayerTime prayerTime, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
