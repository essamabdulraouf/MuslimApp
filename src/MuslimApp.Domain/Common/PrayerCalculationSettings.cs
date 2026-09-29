using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Common;

/// <summary>
/// Everything the calculation engine needs, decoupled from how the choice was persisted. Custom
/// angles are only honoured when <see cref="Method"/> is <see cref="CalculationMethod.Custom"/>.
/// </summary>
public sealed record PrayerCalculationSettings
{
    public required CalculationMethod Method { get; init; }

    public Madhab Madhab { get; init; } = Madhab.Shafi;

    public HighLatitudeRule HighLatitudeRule { get; init; } = HighLatitudeRule.MiddleOfTheNight;

    /// <summary>IANA or Windows time-zone id. When null the engine falls back to UTC.</summary>
    public string? TimeZoneId { get; init; }

    public MinuteAdjustments Adjustments { get; init; } = MinuteAdjustments.None;

    /// <summary>Sun depression below the horizon for Fajr, in degrees.</summary>
    public double? CustomFajrAngle { get; init; }

    /// <summary>Sun depression below the horizon for Isha, in degrees.</summary>
    public double? CustomIshaAngle { get; init; }

    /// <summary>
    /// Fixed interval after Maghrib for Isha, used by Umm al-Qura and Gulf conventions and available
    /// to custom configurations.
    /// </summary>
    public int? CustomIshaIntervalMinutes { get; init; }

    public static PrayerCalculationSettings Default { get; } = new() { Method = CalculationMethod.UmmAlQura };
}

/// <summary>
/// Per-prayer manual corrections in whole minutes, applied after the astronomical result.
/// </summary>
public readonly record struct MinuteAdjustments(
    int Fajr = 0,
    int Sunrise = 0,
    int Dhuhr = 0,
    int Asr = 0,
    int Maghrib = 0,
    int Isha = 0)
{
    public static MinuteAdjustments None { get; } = new();

    public int this[PrayerType type] => type switch
    {
        PrayerType.Fajr => Fajr,
        PrayerType.Sunrise => Sunrise,
        PrayerType.Dhuhr => Dhuhr,
        PrayerType.Asr => Asr,
        PrayerType.Maghrib => Maghrib,
        PrayerType.Isha => Isha,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown prayer type.")
    };
}
