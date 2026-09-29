namespace MuslimApp.Domain.Entities.Prayer;

/// <summary>
/// A cached daily timetable row. Calculation is cheap, so this exists to keep the notification
/// scheduler and the monthly calendar from recomputing, not because the result is expensive.
/// </summary>
public class PrayerTime
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public DateTimeOffset Fajr { get; set; }

    public DateTimeOffset Sunrise { get; set; }

    public DateTimeOffset Dhuhr { get; set; }

    public DateTimeOffset Asr { get; set; }

    public DateTimeOffset Maghrib { get; set; }

    public DateTimeOffset Isha { get; set; }
}
