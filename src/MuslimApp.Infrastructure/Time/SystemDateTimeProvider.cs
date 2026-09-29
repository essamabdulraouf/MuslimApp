using MuslimApp.Application.Abstractions.Platform;

namespace MuslimApp.Infrastructure.Time;

/// <summary>
/// The real clock. The only place in the app that reads ambient time, so everything above it can be
/// tested at a fixed instant.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    public TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;
}
