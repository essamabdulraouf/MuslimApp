using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Settings;

namespace MuslimApp.Mobile.Services;

/// <summary>
/// A placeholder for local notification delivery, which lands in M3.
/// </summary>
/// <remarks>
/// <para>
/// M1 establishes the contract and the permission flow; the platform schedulers (Android's
/// <c>AlarmManager</c> with exact-alarm handling, and <c>UNUserNotificationCenter</c> on iOS) are deliberately
/// not written yet, because doing them properly means dealing with doze mode, exact-alarm policy, boot
/// rescheduling and adhan audio channels — a milestone of its own rather than a footnote to the foundation.
/// </para>
/// <para>
/// It records what it was asked to do at debug level and does nothing else. It never throws, so no caller has
/// to know whether delivery exists yet. The one thing it must not do is make the interface look implemented:
/// the settings screen therefore does not offer notification toggles in this milestone.
/// </para>
/// </remarks>
internal sealed class DeferredNotificationService(ILogger<DeferredNotificationService> logger) : INotificationService
{
    public Task SchedulePrayerNotificationsAsync(
        PrayerTimes prayerTimes,
        NotificationSettings settings,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Prayer notifications for {Date} were requested; scheduling arrives in M3.", prayerTimes.Date);
        return Task.CompletedTask;
    }

    public Task ScheduleAzkarNotificationsAsync(NotificationSettings settings, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Azkar notifications were requested; scheduling arrives in M3.");
        return Task.CompletedTask;
    }

    public Task CancelAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Cancelling notifications was requested; scheduling arrives in M3.");
        return Task.CompletedTask;
    }
}
