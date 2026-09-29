using MuslimApp.Application.DTOs;
using MuslimApp.Domain.Common;

namespace MuslimApp.Application.Abstractions.Services;

/// <summary>
/// What the prayer screens talk to. It hides where the coordinates came from, which convention is
/// configured, and whether the result was cached.
/// </summary>
public interface IPrayerService
{
    Task<Result<PrayerDayDto>> GetDayAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<Result<PrayerDayDto>> GetTodayAsync(CancellationToken cancellationToken = default);

    Task<Result<NextPrayerDto>> GetNextPrayerAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PrayerDayDto>>> GetMonthAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops the cached timetable. Called whenever location or calculation settings change, so a stale
    /// day can never be shown.
    /// </summary>
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
