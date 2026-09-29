using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Enums;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Application.Features.Settings;

/// <summary>
/// The single writer for user configuration. Every mutation persists and then announces what changed,
/// which is how the timetable cache and the notification schedule stay consistent without pages having
/// to remember to refresh anything.
/// </summary>
public sealed class SettingsService(
    ISettingsRepository repository,
    IDateTimeProvider dateTimeProvider,
    ILogger<SettingsService> logger) : ISettingsService
{
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public Task<AppSettings> GetAppSettingsAsync(CancellationToken cancellationToken = default) =>
        repository.GetAppSettingsAsync(cancellationToken);

    public Task<PrayerSettings> GetPrayerSettingsAsync(CancellationToken cancellationToken = default) =>
        repository.GetPrayerSettingsAsync(cancellationToken);

    public Task<NotificationSettings> GetNotificationSettingsAsync(CancellationToken cancellationToken = default) =>
        repository.GetNotificationSettingsAsync(cancellationToken);

    public async Task SetLanguageAsync(Language language, CancellationToken cancellationToken = default)
    {
        AppSettings settings = await repository.GetAppSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.Language == language)
        {
            return;
        }

        settings.Language = language;
        await repository.SaveAppSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.Presentation);
    }

    public async Task SetThemeAsync(ThemeMode theme, CancellationToken cancellationToken = default)
    {
        AppSettings settings = await repository.GetAppSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.Theme == theme)
        {
            return;
        }

        settings.Theme = theme;
        await repository.SaveAppSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.Presentation);
    }

    public async Task SetCalculationMethodAsync(CalculationMethod method, CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await repository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.CalculationMethod == method)
        {
            return;
        }

        settings.CalculationMethod = method;
        settings.UpdatedAt = dateTimeProvider.Now;
        await repository.SavePrayerSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.PrayerCalculation);
    }

    public async Task SetMadhabAsync(Madhab madhab, CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await repository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.Madhab == madhab)
        {
            return;
        }

        settings.Madhab = madhab;
        settings.UpdatedAt = dateTimeProvider.Now;
        await repository.SavePrayerSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.PrayerCalculation);
    }

    public async Task SetAdjustmentsAsync(MinuteAdjustments adjustments, CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await repository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);

        settings.FajrAdjustment = adjustments.Fajr;
        settings.SunriseAdjustment = adjustments.Sunrise;
        settings.DhuhrAdjustment = adjustments.Dhuhr;
        settings.AsrAdjustment = adjustments.Asr;
        settings.MaghribAdjustment = adjustments.Maghrib;
        settings.IshaAdjustment = adjustments.Isha;
        settings.UpdatedAt = dateTimeProvider.Now;

        await repository.SavePrayerSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.PrayerCalculation);
    }

    public async Task SetLocationAsync(LocationResult location, CancellationToken cancellationToken = default)
    {
        PrayerSettings settings = await repository.GetPrayerSettingsAsync(cancellationToken).ConfigureAwait(false);

        settings.Latitude = location.Coordinates.Latitude;
        settings.Longitude = location.Coordinates.Longitude;
        settings.Elevation = location.Coordinates.Elevation;
        settings.LocationName = location.LocationName ?? settings.LocationName;
        settings.TimeZoneId = location.TimeZoneId ?? settings.TimeZoneId;
        settings.HasLocation = true;
        settings.UpdatedAt = dateTimeProvider.Now;

        await repository.SavePrayerSettingsAsync(settings, cancellationToken).ConfigureAwait(false);

        // Coordinates are deliberately not logged.
        logger.LogInformation("Prayer location updated from {Source}.", location.Source);
        Raise(SettingsChangeKind.Location | SettingsChangeKind.PrayerCalculation);
    }

    public async Task SaveNotificationSettingsAsync(NotificationSettings settings, CancellationToken cancellationToken = default)
    {
        await repository.SaveNotificationSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.Notifications);
    }

    public async Task CompleteOnboardingAsync(CancellationToken cancellationToken = default)
    {
        AppSettings settings = await repository.GetAppSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.FirstLaunchCompleted)
        {
            return;
        }

        settings.FirstLaunchCompleted = true;
        await repository.SaveAppSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
        Raise(SettingsChangeKind.Onboarding);
    }

    public async Task MarkLocationPermissionRequestedAsync(CancellationToken cancellationToken = default)
    {
        AppSettings settings = await repository.GetAppSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.LocationPermissionRequested)
        {
            return;
        }

        settings.LocationPermissionRequested = true;
        await repository.SaveAppSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkNotificationPermissionRequestedAsync(CancellationToken cancellationToken = default)
    {
        AppSettings settings = await repository.GetAppSettingsAsync(cancellationToken).ConfigureAwait(false);
        if (settings.NotificationPermissionRequested)
        {
            return;
        }

        settings.NotificationPermissionRequested = true;
        await repository.SaveAppSettingsAsync(settings, cancellationToken).ConfigureAwait(false);
    }

    private void Raise(SettingsChangeKind kind) => SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(kind));
}
