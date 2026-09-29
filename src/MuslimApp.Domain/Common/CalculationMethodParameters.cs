using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Common;

/// <summary>
/// The twilight conventions behind each <see cref="CalculationMethod"/>. These are religious
/// conventions published by the bodies they are named after, not tuning knobs: changing a value
/// silently changes every user's timetable, so each one is sourced and covered by a test.
/// </summary>
public sealed record CalculationMethodParameters
{
    public required CalculationMethod Method { get; init; }

    /// <summary>Sun depression below the horizon at Fajr, in degrees.</summary>
    public required double FajrAngle { get; init; }

    /// <summary>Sun depression at Isha. Null when the method uses a fixed interval instead.</summary>
    public double? IshaAngle { get; init; }

    /// <summary>Fixed minutes after Maghrib for Isha. Null when the method uses an angle.</summary>
    public int? IshaIntervalMinutes { get; init; }

    /// <summary>Corrections the convention itself prescribes, applied before the user's own adjustments.</summary>
    public MinuteAdjustments MethodAdjustments { get; init; } = MinuteAdjustments.None;

    private static readonly Dictionary<CalculationMethod, CalculationMethodParameters> Catalog = new()
    {
        // Umm al-Qura University, Mecca. Isha is a fixed 90 minutes after Maghrib.
        [CalculationMethod.UmmAlQura] = new()
        {
            Method = CalculationMethod.UmmAlQura,
            FajrAngle = 18.5d,
            IshaIntervalMinutes = 90
        },

        // Muslim World League.
        [CalculationMethod.MuslimWorldLeague] = new()
        {
            Method = CalculationMethod.MuslimWorldLeague,
            FajrAngle = 18d,
            IshaAngle = 17d,
            MethodAdjustments = new MinuteAdjustments(Dhuhr: 1)
        },

        // Egyptian General Authority of Survey.
        [CalculationMethod.Egyptian] = new()
        {
            Method = CalculationMethod.Egyptian,
            FajrAngle = 19.5d,
            IshaAngle = 17.5d,
            MethodAdjustments = new MinuteAdjustments(Dhuhr: 1)
        },

        // University of Islamic Sciences, Karachi.
        [CalculationMethod.Karachi] = new()
        {
            Method = CalculationMethod.Karachi,
            FajrAngle = 18d,
            IshaAngle = 18d,
            MethodAdjustments = new MinuteAdjustments(Dhuhr: 1)
        },

        // Islamic Society of North America.
        [CalculationMethod.Isna] = new()
        {
            Method = CalculationMethod.Isna,
            FajrAngle = 15d,
            IshaAngle = 15d,
            MethodAdjustments = new MinuteAdjustments(Dhuhr: 1)
        },

        // Gulf region convention.
        [CalculationMethod.Gulf] = new()
        {
            Method = CalculationMethod.Gulf,
            FajrAngle = 19.5d,
            IshaIntervalMinutes = 90
        }
    };

    public static IReadOnlyCollection<CalculationMethodParameters> All => Catalog.Values;

    public static CalculationMethodParameters For(CalculationMethod method) =>
        Catalog.TryGetValue(method, out CalculationMethodParameters? parameters)
            ? parameters
            : throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "This method has no published parameters; supply them through PrayerCalculationSettings instead.");

    /// <summary>
    /// Turns user settings into concrete parameters. A <see cref="CalculationMethod.Custom"/> selection
    /// must supply a Fajr angle plus either an Isha angle or an Isha interval.
    /// </summary>
    public static CalculationMethodParameters Resolve(PrayerCalculationSettings settings)
    {
        if (settings.Method != CalculationMethod.Custom)
        {
            CalculationMethodParameters published = For(settings.Method);

            // A user may still override the angles of a published method.
            if (settings.CustomFajrAngle is null && settings.CustomIshaAngle is null && settings.CustomIshaIntervalMinutes is null)
            {
                return published;
            }

            return published with
            {
                FajrAngle = settings.CustomFajrAngle ?? published.FajrAngle,
                IshaAngle = settings.CustomIshaIntervalMinutes is not null ? null : settings.CustomIshaAngle ?? published.IshaAngle,
                IshaIntervalMinutes = settings.CustomIshaIntervalMinutes ?? (settings.CustomIshaAngle is not null ? null : published.IshaIntervalMinutes)
            };
        }

        if (settings.CustomFajrAngle is null)
        {
            throw new InvalidOperationException("A custom calculation method requires a Fajr angle.");
        }

        if (settings.CustomIshaAngle is null && settings.CustomIshaIntervalMinutes is null)
        {
            throw new InvalidOperationException("A custom calculation method requires either an Isha angle or an Isha interval.");
        }

        return new CalculationMethodParameters
        {
            Method = CalculationMethod.Custom,
            FajrAngle = settings.CustomFajrAngle.Value,
            IshaAngle = settings.CustomIshaAngle,
            IshaIntervalMinutes = settings.CustomIshaIntervalMinutes
        };
    }
}
