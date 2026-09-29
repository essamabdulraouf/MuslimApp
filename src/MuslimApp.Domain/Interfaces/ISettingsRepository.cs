using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;

namespace MuslimApp.Domain.Interfaces;

/// <summary>
/// Access to the single-row settings tables. Getters always return a row: the repository materializes
/// defaults on first use so the rest of the app never handles a "settings missing" case.
/// </summary>
public interface ISettingsRepository
{
    Task<AppSettings> GetAppSettingsAsync(CancellationToken cancellationToken = default);

    Task SaveAppSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);

    Task<PrayerSettings> GetPrayerSettingsAsync(CancellationToken cancellationToken = default);

    Task SavePrayerSettingsAsync(PrayerSettings settings, CancellationToken cancellationToken = default);

    Task<NotificationSettings> GetNotificationSettingsAsync(CancellationToken cancellationToken = default);

    Task SaveNotificationSettingsAsync(NotificationSettings settings, CancellationToken cancellationToken = default);
}
