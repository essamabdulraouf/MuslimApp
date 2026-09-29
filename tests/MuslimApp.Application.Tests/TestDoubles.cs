using MuslimApp.Application.Abstractions.Platform;

namespace MuslimApp.Application.Tests;

/// <summary>A clock frozen at a chosen instant, in a chosen zone.</summary>
public sealed class FixedDateTimeProvider(DateTimeOffset now, TimeZoneInfo? zone = null) : IDateTimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public TimeZoneInfo LocalTimeZone { get; } = zone ?? TimeZoneInfo.Utc;

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Now, LocalTimeZone).DateTime);
}
