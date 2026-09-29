using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Common;

/// <summary>
/// The computed daily timetable for one location. Every instant carries the offset that was in
/// effect at that moment, so a day containing a DST transition stays correct.
/// </summary>
public sealed class PrayerTimes
{
    private static readonly PrayerType[] OrderedTypes =
    [
        PrayerType.Fajr,
        PrayerType.Sunrise,
        PrayerType.Dhuhr,
        PrayerType.Asr,
        PrayerType.Maghrib,
        PrayerType.Isha
    ];

    public PrayerTimes(
        DateOnly date,
        DateTimeOffset fajr,
        DateTimeOffset sunrise,
        DateTimeOffset dhuhr,
        DateTimeOffset asr,
        DateTimeOffset maghrib,
        DateTimeOffset isha,
        DateTimeOffset midnight)
    {
        Date = date;
        Fajr = fajr;
        Sunrise = sunrise;
        Dhuhr = dhuhr;
        Asr = asr;
        Maghrib = maghrib;
        Isha = isha;
        Midnight = midnight;
    }

    public DateOnly Date { get; }

    public DateTimeOffset Fajr { get; }

    public DateTimeOffset Sunrise { get; }

    public DateTimeOffset Dhuhr { get; }

    public DateTimeOffset Asr { get; }

    public DateTimeOffset Maghrib { get; }

    public DateTimeOffset Isha { get; }

    /// <summary>
    /// The midpoint between Maghrib and the following Fajr. Not one of the five prayers; exposed
    /// because the night is split relative to it (qiyam, last third).
    /// </summary>
    public DateTimeOffset Midnight { get; }

    public static IReadOnlyList<PrayerType> Order => OrderedTypes;

    public DateTimeOffset this[PrayerType type] => type switch
    {
        PrayerType.Fajr => Fajr,
        PrayerType.Sunrise => Sunrise,
        PrayerType.Dhuhr => Dhuhr,
        PrayerType.Asr => Asr,
        PrayerType.Maghrib => Maghrib,
        PrayerType.Isha => Isha,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown prayer type.")
    };

    /// <summary>
    /// The first entry strictly after <paramref name="instant"/>, or <see langword="null"/> when the
    /// day is already finished. Callers roll over to the next day themselves.
    /// </summary>
    public (PrayerType Type, DateTimeOffset Time)? GetNext(DateTimeOffset instant)
    {
        foreach (PrayerType type in OrderedTypes)
        {
            DateTimeOffset time = this[type];
            if (time > instant)
            {
                return (type, time);
            }
        }

        return null;
    }

    /// <summary>
    /// The entry whose window contains <paramref name="instant"/>, or <see langword="null"/> when
    /// the instant falls before Fajr of this day.
    /// </summary>
    public (PrayerType Type, DateTimeOffset Time)? GetCurrent(DateTimeOffset instant)
    {
        for (int i = OrderedTypes.Length - 1; i >= 0; i--)
        {
            DateTimeOffset time = this[OrderedTypes[i]];
            if (time <= instant)
            {
                return (OrderedTypes[i], time);
            }
        }

        return null;
    }
}
