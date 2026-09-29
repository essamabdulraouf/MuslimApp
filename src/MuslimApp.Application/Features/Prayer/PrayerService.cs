using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.DTOs;
using MuslimApp.Application.Mappers;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Enums;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Application.Features.Prayer;

/// <summary>
/// Orchestrates the timetable: read settings, calculate, project for the UI.
/// </summary>
/// <remarks>
/// Display always recalculates rather than reading the cache. A day costs well under a millisecond, so
/// caching for display would only create a way to show a stale timetable after a settings or location
/// change. The cache is written through for consumers that cannot recalculate on demand — notably the
/// notification scheduler — and is cleared by <see cref="InvalidateAsync"/>.
/// </remarks>
public sealed class PrayerService(
    ISettingsRepository settingsRepository,
    IPrayerTimesRepository prayerTimesRepository,
    IPrayerCalculationService calculationService,
    IDateTimeProvider dateTimeProvider,
    ILogger<PrayerService> logger) : IPrayerService
{
    public async Task<Result<PrayerDayDto>> GetDayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await settingsRepository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);

        if (!settings.HasLocation)
        {
            return Result.Failure<PrayerDayDto>(Error.NotConfigured);
        }

        PrayerTimes times = Calculate(settings, date);
        await WriteThroughAsync(times, cancellationToken).ConfigureAwait(false);

        return Result.Success(times.ToDto(dateTimeProvider.Now, settings));
    }

    public async Task<Result<PrayerDayDto>> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await settingsRepository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);
        return await GetDayAsync(LocalToday(settings), cancellationToken).ConfigureAwait(false);
    }

    public async Task<Result<NextPrayerDto>> GetNextPrayerAsync(CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await settingsRepository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);

        if (!settings.HasLocation)
        {
            return Result.Failure<NextPrayerDto>(Error.NotConfigured);
        }

        DateTimeOffset now = dateTimeProvider.Now;
        DateOnly today = LocalToday(settings);

        PrayerTimes todayTimes = Calculate(settings, today);
        (PrayerType Type, DateTimeOffset Time)? next = todayTimes.GetNext(now);

        if (next is not null)
        {
            return Result.Success(new NextPrayerDto(
                next.Value.Type,
                next.Value.Time,
                next.Value.Time - now,
                IsTomorrow: false));
        }

        // Isha has passed: the next entry is tomorrow's Fajr, which must be calculated for tomorrow's
        // date so a DST change overnight is reflected.
        PrayerTimes tomorrowTimes = Calculate(settings, today.AddDays(1));
        return Result.Success(new NextPrayerDto(
            PrayerType.Fajr,
            tomorrowTimes.Fajr,
            tomorrowTimes.Fajr - now,
            IsTomorrow: true));
    }

    public async Task<Result<IReadOnlyList<PrayerDayDto>>> GetMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        if (year is < 1 or > 9999 || month is < 1 or > 12)
        {
            return Result.Failure<IReadOnlyList<PrayerDayDto>>(new Error("prayer.invalid_month", "The requested month is out of range."));
        }

        PrayerSettings settings = await settingsRepository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);

        if (!settings.HasLocation)
        {
            return Result.Failure<IReadOnlyList<PrayerDayDto>>(Error.NotConfigured);
        }

        DateTimeOffset now = dateTimeProvider.Now;
        int dayCount = DateTime.DaysInMonth(year, month);
        var days = new List<PrayerDayDto>(dayCount);

        for (int day = 1; day <= dayCount; day++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PrayerTimes times = Calculate(settings, new DateOnly(year, month, day));
            days.Add(times.ToDto(now, settings));
        }

        return Result.Success<IReadOnlyList<PrayerDayDto>>(days);
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Clearing the cached prayer timetable after a configuration change.");
        return prayerTimesRepository.ClearAsync(cancellationToken);
    }

    private PrayerTimes Calculate(PrayerSettings settings, DateOnly date) =>
        calculationService.Calculate(settings.ToCoordinates(), date, settings.ToCalculationSettings());

    /// <summary>
    /// Today in the *configured* zone. Someone in London who has pinned their location to Riyadh should
    /// see Riyadh's day, not the device's.
    /// </summary>
    private DateOnly LocalToday(PrayerSettings settings)
    {
        TimeZoneInfo zone = ResolveZone(settings.TimeZoneId);
        DateTimeOffset now = dateTimeProvider.Now;
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
    }

    private TimeZoneInfo ResolveZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return dateTimeProvider.LocalTimeZone;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            logger.LogWarning("Configured time zone {TimeZoneId} is not available on this device; using the device zone.", timeZoneId);
            return dateTimeProvider.LocalTimeZone;
        }
    }

    private async Task WriteThroughAsync(PrayerTimes times, CancellationToken cancellationToken)
    {
        try
        {
            await prayerTimesRepository.UpsertAsync(times.ToEntity(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The cache is derived data. Failing to persist it must never stop the user from seeing times.
            logger.LogWarning(ex, "Could not cache the timetable for {Date}.", times.Date);
        }
    }
}
