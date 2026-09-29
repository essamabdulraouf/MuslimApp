using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;

namespace MuslimApp.Application.Abstractions.Platform;

/// <summary>
/// Device positioning. Returns a <see cref="Result{T}"/> rather than throwing, because "no fix
/// available" is an ordinary outcome that the UI must handle gracefully.
/// </summary>
public interface ILocationService
{
    Task<Result<LocationResult>> GetCurrentLocationAsync(CancellationToken cancellationToken = default);

    /// <summary>The last fix the platform still remembers. Fast, and may be stale.</summary>
    Task<Result<LocationResult>> GetLastKnownLocationAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Runtime permission state. Separated from the services that need permissions so onboarding can ask
/// once, up front, without touching sensors.
/// </summary>
public interface IPermissionService
{
    Task<bool> IsLocationGrantedAsync();

    Task<bool> RequestLocationAsync();

    Task<bool> IsNotificationGrantedAsync();

    Task<bool> RequestNotificationAsync();
}

/// <summary>
/// Local notification scheduling. Every notification in this app is scheduled on-device; no server is
/// involved, so the app keeps working offline and no schedule ever leaves the phone.
/// </summary>
public interface INotificationService
{
    Task SchedulePrayerNotificationsAsync(PrayerTimes prayerTimes, NotificationSettings settings, CancellationToken cancellationToken = default);

    Task ScheduleAzkarNotificationsAsync(NotificationSettings settings, CancellationToken cancellationToken = default);

    Task CancelAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// The clock, injected so that timetable and "next prayer" behaviour is deterministic under test.
/// Nothing in Application or Domain reads <c>DateTime.Now</c> directly.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset Now { get; }

    DateOnly Today { get; }

    TimeZoneInfo LocalTimeZone { get; }
}

/// <summary>
/// Network reachability, used only to decide whether an *optional* online action is worth attempting.
/// No required feature may consult this.
/// </summary>
public interface IConnectivityService
{
    bool IsConnected { get; }
}
