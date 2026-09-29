namespace MuslimApp.Domain.Entities.Settings;

/// <summary>
/// Single-row notification preferences. Do-not-disturb is stored as a local time-of-day window and is
/// evaluated against the device's current zone, not the zone it was configured in.
/// </summary>
public class NotificationSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public bool PrayerNotifications { get; set; } = true;

    public bool AdhanEnabled { get; set; } = true;

    public bool PrayerReminderEnabled { get; set; }

    public int PrayerReminderMinutesBefore { get; set; } = 10;

    public bool MorningAzkarEnabled { get; set; }

    public bool EveningAzkarEnabled { get; set; }

    public bool AfterPrayerAzkarEnabled { get; set; }

    public bool TasbihReminderEnabled { get; set; }

    public bool DoNotDisturbEnabled { get; set; }

    public TimeOnly DoNotDisturbFrom { get; set; } = new(22, 0);

    public TimeOnly DoNotDisturbTo { get; set; } = new(6, 0);

    /// <summary>Identifier of the selected adhan audio asset; null means the default muadhin.</summary>
    public string? AdhanSoundId { get; set; }

    /// <summary>
    /// True when the window wraps past midnight, which is the normal case for a night-time window.
    /// </summary>
    public bool DoNotDisturbCrossesMidnight => DoNotDisturbFrom > DoNotDisturbTo;

    public bool IsWithinDoNotDisturb(TimeOnly localTime)
    {
        if (!DoNotDisturbEnabled || DoNotDisturbFrom == DoNotDisturbTo)
        {
            return false;
        }

        return DoNotDisturbCrossesMidnight
            ? localTime >= DoNotDisturbFrom || localTime < DoNotDisturbTo
            : localTime >= DoNotDisturbFrom && localTime < DoNotDisturbTo;
    }
}
