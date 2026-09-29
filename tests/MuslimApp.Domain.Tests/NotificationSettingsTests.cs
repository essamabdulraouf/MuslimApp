using MuslimApp.Domain.Entities.Settings;

namespace MuslimApp.Domain.Tests;

public class NotificationSettingsTests
{
    [Theory]
    [InlineData(22, 30, true)]
    [InlineData(23, 59, true)]
    [InlineData(0, 0, true)]
    [InlineData(5, 59, true)]
    [InlineData(6, 0, false)]
    [InlineData(12, 0, false)]
    [InlineData(21, 59, false)]
    [InlineData(22, 0, true)]
    public void A_night_window_wraps_past_midnight(int hour, int minute, bool expected)
    {
        var settings = new NotificationSettings
        {
            DoNotDisturbEnabled = true,
            DoNotDisturbFrom = new TimeOnly(22, 0),
            DoNotDisturbTo = new TimeOnly(6, 0)
        };

        settings.DoNotDisturbCrossesMidnight.Should().BeTrue();
        settings.IsWithinDoNotDisturb(new TimeOnly(hour, minute)).Should().Be(expected);
    }

    [Theory]
    [InlineData(13, 30, true)]
    [InlineData(12, 0, true)]
    [InlineData(15, 0, false)]
    [InlineData(11, 59, false)]
    public void A_daytime_window_does_not_wrap(int hour, int minute, bool expected)
    {
        var settings = new NotificationSettings
        {
            DoNotDisturbEnabled = true,
            DoNotDisturbFrom = new TimeOnly(12, 0),
            DoNotDisturbTo = new TimeOnly(15, 0)
        };

        settings.DoNotDisturbCrossesMidnight.Should().BeFalse();
        settings.IsWithinDoNotDisturb(new TimeOnly(hour, minute)).Should().Be(expected);
    }

    [Fact]
    public void A_disabled_window_never_suppresses_anything()
    {
        var settings = new NotificationSettings
        {
            DoNotDisturbEnabled = false,
            DoNotDisturbFrom = new TimeOnly(22, 0),
            DoNotDisturbTo = new TimeOnly(6, 0)
        };

        settings.IsWithinDoNotDisturb(new TimeOnly(23, 0)).Should().BeFalse();
    }

    [Fact]
    public void An_empty_window_never_suppresses_anything()
    {
        var settings = new NotificationSettings
        {
            DoNotDisturbEnabled = true,
            DoNotDisturbFrom = new TimeOnly(22, 0),
            DoNotDisturbTo = new TimeOnly(22, 0)
        };

        settings.IsWithinDoNotDisturb(new TimeOnly(22, 0)).Should().BeFalse();
    }

    [Fact]
    public void Prayer_notifications_are_on_by_default_and_azkar_reminders_are_not()
    {
        var settings = new NotificationSettings();

        settings.PrayerNotifications.Should().BeTrue();
        settings.AdhanEnabled.Should().BeTrue();
        settings.MorningAzkarEnabled.Should().BeFalse("opt-in reminders must not be switched on for the user");
        settings.EveningAzkarEnabled.Should().BeFalse();
        settings.TasbihReminderEnabled.Should().BeFalse();
    }
}
