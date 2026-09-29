using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Application.Abstractions.Services;

/// <summary>
/// Reads and writes user configuration. Raises <see cref="SettingsChanged"/> so dependent caches (the
/// prayer timetable, the notification schedule) can be rebuilt without pages coordinating it.
/// </summary>
public interface ISettingsService
{
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    Task<AppSettings> GetAppSettingsAsync(CancellationToken cancellationToken = default);

    Task<PrayerSettings> GetPrayerSettingsAsync(CancellationToken cancellationToken = default);

    Task<NotificationSettings> GetNotificationSettingsAsync(CancellationToken cancellationToken = default);

    Task SetLanguageAsync(Language language, CancellationToken cancellationToken = default);

    Task SetThemeAsync(ThemeMode theme, CancellationToken cancellationToken = default);

    Task SetCalculationMethodAsync(CalculationMethod method, CancellationToken cancellationToken = default);

    Task SetMadhabAsync(Madhab madhab, CancellationToken cancellationToken = default);

    Task SetAdjustmentsAsync(MinuteAdjustments adjustments, CancellationToken cancellationToken = default);

    Task SetLocationAsync(LocationResult location, CancellationToken cancellationToken = default);

    Task SaveNotificationSettingsAsync(NotificationSettings settings, CancellationToken cancellationToken = default);

    Task CompleteOnboardingAsync(CancellationToken cancellationToken = default);

    Task MarkLocationPermissionRequestedAsync(CancellationToken cancellationToken = default);

    Task MarkNotificationPermissionRequestedAsync(CancellationToken cancellationToken = default);
}

/// <summary>Which area of configuration changed, so listeners can ignore changes they do not care about.</summary>
[Flags]
public enum SettingsChangeKind
{
    None = 0,
    Presentation = 1,
    Location = 2,
    PrayerCalculation = 4,
    Notifications = 8,
    Onboarding = 16
}

public sealed class SettingsChangedEventArgs(SettingsChangeKind kind) : EventArgs
{
    public SettingsChangeKind Kind { get; } = kind;
}
