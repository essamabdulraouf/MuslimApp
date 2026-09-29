using MuslimApp.Domain.Enums;

namespace MuslimApp.Application.DTOs;

/// <summary>One row of the daily timetable, already ordered and flagged for the UI.</summary>
public sealed record PrayerEntryDto(
    PrayerType Type,
    DateTimeOffset Time,
    bool IsPassed,
    bool IsNext,
    bool IsObligatory);

/// <summary>
/// A full day for one location. <paramref name="LocationName"/> is null when the user has not chosen a
/// city and no fix was available.
/// </summary>
public sealed record PrayerDayDto(
    DateOnly Date,
    string? LocationName,
    CalculationMethod Method,
    Madhab Madhab,
    IReadOnlyList<PrayerEntryDto> Entries,
    DateTimeOffset Midnight);

/// <summary>
/// The upcoming prayer. <paramref name="IsTomorrow"/> is true once the current day's Isha has passed
/// and the next entry is tomorrow's Fajr.
/// </summary>
public sealed record NextPrayerDto(
    PrayerType Type,
    DateTimeOffset Time,
    TimeSpan TimeRemaining,
    bool IsTomorrow);

/// <summary>A selectable calculation convention, for the settings and onboarding pickers.</summary>
public sealed record CalculationMethodOptionDto(
    CalculationMethod Method,
    string ResourceKey,
    double FajrAngle,
    double? IshaAngle,
    int? IshaIntervalMinutes);
