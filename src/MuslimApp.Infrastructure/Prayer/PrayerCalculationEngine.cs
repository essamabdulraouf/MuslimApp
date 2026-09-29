using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Prayer;

/// <summary>
/// Turns a position, a date and a convention into a timetable.
/// </summary>
/// <remarks>
/// <para>
/// The calculation is pure and has no dependencies: no clock, no database, no device. That is
/// deliberate, because this class is the app's single trust promise and every behaviour below is pinned
/// by a test against published timetables.
/// </para>
/// <para>
/// Sequence for one day: solar transit gives Dhuhr; the sun crossing the apparent horizon gives Sunrise
/// and Maghrib; the Asr shadow ratio gives Asr; the method's twilight angles give Fajr and Isha. Where
/// the sun never reaches a twilight angle, the configured <see cref="HighLatitudeRule"/> divides the
/// night instead. Above the polar circles, where the sun may not cross the horizon at all, the
/// nearest-latitude rule substitutes 48.5 degrees for the horizon-dependent events so the app always
/// produces an ordered, usable timetable rather than failing.
/// </para>
/// </remarks>
public sealed class PrayerCalculationEngine : IPrayerCalculationService
{
    /// <summary>
    /// Apparent altitude of the sun's upper limb at sunrise and sunset: -50 arc-minutes, which is the
    /// standard allowance for refraction plus the solar semi-diameter.
    /// </summary>
    private const double HorizonAltitude = -50d / 60d;

    /// <summary>
    /// Latitude substituted for horizon-dependent events when the sun does not rise or set. 48.5 degrees
    /// is the conventional choice for the nearest-latitude rule.
    /// </summary>
    private const double NearestLatitudeLimit = 48.5d;

    private const double HanafiShadowLength = 2d;
    private const double StandardShadowLength = 1d;

    public PrayerTimes Calculate(Coordinates coordinates, DateOnly date, PrayerCalculationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        CalculationMethodParameters parameters = CalculationMethodParameters.Resolve(settings);
        TimeZoneInfo zone = ResolveTimeZone(settings.TimeZoneId);

        DayTimes today = ComputeDay(coordinates, date, parameters, settings);
        DayTimes tomorrow = ComputeDay(coordinates, date.AddDays(1), parameters, settings);

        // The night belonging to this day runs from its Maghrib to the following Fajr.
        long halfNightTicks = (tomorrow.Fajr - today.Maghrib).Ticks / 2L;
        DateTime midnight = RoundToMinute(today.Maghrib.AddTicks(halfNightTicks));

        return new PrayerTimes(
            date,
            ToZone(today.Fajr, zone),
            ToZone(today.Sunrise, zone),
            ToZone(today.Dhuhr, zone),
            ToZone(today.Asr, zone),
            ToZone(today.Maghrib, zone),
            ToZone(today.Isha, zone),
            ToZone(midnight, zone));
    }

    private static DayTimes ComputeDay(
        Coordinates coordinates,
        DateOnly date,
        CalculationMethodParameters parameters,
        PrayerCalculationSettings settings)
    {
        // Higher ground sees the sun earlier and loses it later.
        double horizon = HorizonAltitude - (0.0347d * Math.Sqrt(Math.Max(0d, coordinates.Elevation)));

        HorizonDay today = ResolveHorizon(date, coordinates, horizon);
        HorizonDay tomorrow = ResolveHorizon(date.AddDays(1), coordinates, horizon);

        DateTime sunrise = FromHours(date, today.Sunrise);
        DateTime sunset = FromHours(date, today.Sunset);
        DateTime dhuhr = FromHours(date, today.Solar.Transit);
        TimeSpan night = FromHours(date.AddDays(1), tomorrow.Sunrise) - sunset;

        DateTime asr = ResolveAsr(date, today, settings.Madhab, dhuhr, sunset);
        DateTime fajr = ResolveFajr(date, today, parameters, settings.HighLatitudeRule, sunrise, night);
        DateTime isha = ResolveIsha(date, today, parameters, settings.HighLatitudeRule, sunset, night);

        return new DayTimes(
            Apply(fajr, PrayerType.Fajr, parameters, settings),
            Apply(sunrise, PrayerType.Sunrise, parameters, settings),
            Apply(dhuhr, PrayerType.Dhuhr, parameters, settings),
            Apply(asr, PrayerType.Asr, parameters, settings),
            Apply(sunset, PrayerType.Maghrib, parameters, settings),
            Apply(isha, PrayerType.Isha, parameters, settings));
    }

    /// <summary>
    /// Sunrise and sunset for a day, falling back to the nearest-latitude rule when the sun neither
    /// rises nor sets.
    /// </summary>
    private static HorizonDay ResolveHorizon(DateOnly date, Coordinates coordinates, double horizon)
    {
        var solar = new SolarTime(date, coordinates);
        double sunrise = solar.HourAngle(horizon, afterTransit: false);
        double sunset = solar.HourAngle(horizon, afterTransit: true);

        if (!double.IsNaN(sunrise) && !double.IsNaN(sunset))
        {
            return new HorizonDay(solar, sunrise, sunset);
        }

        double substituteLatitude = Math.Clamp(coordinates.Latitude, -NearestLatitudeLimit, NearestLatitudeLimit);
        var substitute = new SolarTime(date, substituteLatitude, coordinates.Longitude);

        return new HorizonDay(
            substitute,
            substitute.HourAngle(horizon, afterTransit: false),
            substitute.HourAngle(horizon, afterTransit: true));
    }

    private static DateTime ResolveAsr(DateOnly date, HorizonDay day, Madhab madhab, DateTime dhuhr, DateTime sunset)
    {
        double shadowLength = madhab == Madhab.Hanafi ? HanafiShadowLength : StandardShadowLength;
        double hours = day.Solar.Afternoon(shadowLength);

        if (!double.IsNaN(hours))
        {
            return FromHours(date, hours);
        }

        // The Hanafi shadow ratio can be unreachable even at latitudes where the sun does set. Splitting
        // the afternoon keeps the timetable ordered and usable.
        return dhuhr.AddTicks((sunset - dhuhr).Ticks / 2L);
    }

    private static DateTime ResolveFajr(
        DateOnly date,
        HorizonDay day,
        CalculationMethodParameters parameters,
        HighLatitudeRule rule,
        DateTime sunrise,
        TimeSpan night)
    {
        double hours = day.Solar.HourAngle(-parameters.FajrAngle, afterTransit: false);
        DateTime earliestAllowed = sunrise - PortionOfNight(night, rule, parameters.FajrAngle);

        if (double.IsNaN(hours))
        {
            return earliestAllowed;
        }

        DateTime fajr = FromHours(date, hours);
        return fajr < earliestAllowed ? earliestAllowed : fajr;
    }

    private static DateTime ResolveIsha(
        DateOnly date,
        HorizonDay day,
        CalculationMethodParameters parameters,
        HighLatitudeRule rule,
        DateTime sunset,
        TimeSpan night)
    {
        if (parameters.IshaIntervalMinutes is int interval)
        {
            // Umm al-Qura and the Gulf convention use a fixed interval after Maghrib rather than an angle.
            return sunset.AddMinutes(interval);
        }

        double ishaAngle = parameters.IshaAngle
            ?? throw new InvalidOperationException("The calculation parameters define neither an Isha angle nor an Isha interval.");

        double hours = day.Solar.HourAngle(-ishaAngle, afterTransit: true);
        DateTime latestAllowed = sunset + PortionOfNight(night, rule, ishaAngle);

        if (double.IsNaN(hours))
        {
            return latestAllowed;
        }

        DateTime isha = FromHours(date, hours);
        return isha > latestAllowed ? latestAllowed : isha;
    }

    /// <summary>
    /// How much of the night the configured rule allows between the horizon and Fajr/Isha when the
    /// twilight angle is never reached.
    /// </summary>
    private static TimeSpan PortionOfNight(TimeSpan night, HighLatitudeRule rule, double twilightAngle)
    {
        double fraction = rule switch
        {
            HighLatitudeRule.SeventhOfTheNight => 1d / 7d,
            HighLatitudeRule.TwilightAngle => twilightAngle / 60d,
            _ => 1d / 2d
        };

        return TimeSpan.FromTicks((long)(night.Ticks * fraction));
    }

    /// <summary>
    /// Applies the convention's own correction and then the user's, and rounds to the displayed minute.
    /// </summary>
    private static DateTime Apply(
        DateTime value,
        PrayerType type,
        CalculationMethodParameters parameters,
        PrayerCalculationSettings settings)
    {
        int minutes = parameters.MethodAdjustments[type] + settings.Adjustments[type];
        return RoundToMinute(value.AddMinutes(minutes));
    }

    private static DateTime FromHours(DateOnly date, double hours)
    {
        if (double.IsNaN(hours) || double.IsInfinity(hours))
        {
            throw new InvalidOperationException($"A solar event for {date:O} could not be resolved.");
        }

        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(hours);
    }

    private static DateTime RoundToMinute(DateTime value)
    {
        long minutes = (long)Math.Round(value.Ticks / (double)TimeSpan.TicksPerMinute, MidpointRounding.AwayFromZero);
        return new DateTime(minutes * TimeSpan.TicksPerMinute, DateTimeKind.Utc);
    }

    private static DateTimeOffset ToZone(DateTime utc, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTime(new DateTimeOffset(utc, TimeSpan.Zero), zone);

    /// <summary>
    /// Resolves the configured zone. UTC is the fallback rather than the device zone so the engine stays
    /// deterministic; callers persist a zone with the location, so this only matters for a
    /// deliberately zone-less calculation.
    /// </summary>
    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private readonly record struct DayTimes(
        DateTime Fajr,
        DateTime Sunrise,
        DateTime Dhuhr,
        DateTime Asr,
        DateTime Maghrib,
        DateTime Isha);

    private readonly record struct HorizonDay(
        SolarTime Solar,
        double Sunrise,
        double Sunset);
}
