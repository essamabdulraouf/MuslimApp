using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;
using MuslimApp.Infrastructure.Prayer;

namespace MuslimApp.Infrastructure.Tests.Prayer;

/// <summary>
/// The timetable is the app's core trust promise, so these tests cover published reference values,
/// the relationships between conventions, and the awkward cases: time zones, daylight saving, manual
/// corrections and the polar regions.
/// </summary>
public class PrayerCalculationEngineTests
{
    private static readonly PrayerCalculationEngine Engine = new();

    private static readonly Coordinates Mecca = new(21.4225d, 39.8262d);
    private static readonly Coordinates Riyadh = new(24.7136d, 46.6753d);
    private static readonly Coordinates Cairo = new(30.0444d, 31.2357d);
    private static readonly Coordinates London = new(51.5074d, -0.1278d);
    private static readonly Coordinates NewYork = new(40.7128d, -74.0060d);
    private static readonly Coordinates Longyearbyen = new(78.2232d, 15.6267d);

    /// <summary>
    /// Reference values are derived from the solar geometry for each position and date (equation of time,
    /// solar declination, the method's twilight angles) and agree with the published tables for those
    /// cities. The tolerance absorbs the small difference between that derivation and the full
    /// interpolated series the engine uses.
    /// </summary>
    private const int ToleranceMinutes = 3;

    [Fact]
    public void Riyadh_matches_the_published_Umm_al_Qura_timetable()
    {
        PrayerTimes times = Engine.Calculate(
            Riyadh,
            new DateOnly(2026, 9, 29),
            Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));

        AssertLocalTime(times.Fajr, 4, 27);
        AssertLocalTime(times.Sunrise, 5, 44);
        AssertLocalTime(times.Dhuhr, 11, 44);
        AssertLocalTime(times.Asr, 15, 8);
        AssertLocalTime(times.Maghrib, 17, 43);
        AssertLocalTime(times.Isha, 19, 13);
    }

    [Fact]
    public void Mecca_matches_the_published_Umm_al_Qura_timetable()
    {
        PrayerTimes times = Engine.Calculate(
            Mecca,
            new DateOnly(2026, 9, 29),
            Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));

        AssertLocalTime(times.Fajr, 4, 55);
        AssertLocalTime(times.Sunrise, 6, 11);
        AssertLocalTime(times.Dhuhr, 12, 11);
        AssertLocalTime(times.Asr, 15, 35);
        AssertLocalTime(times.Maghrib, 18, 11);
        AssertLocalTime(times.Isha, 19, 41);
    }

    [Fact]
    public void Cairo_matches_the_published_Egyptian_timetable()
    {
        PrayerTimes times = Engine.Calculate(
            Cairo,
            new DateOnly(2026, 1, 15),
            Settings(CalculationMethod.Egyptian, "Africa/Cairo"));

        AssertLocalTime(times.Fajr, 5, 21);
        AssertLocalTime(times.Sunrise, 6, 52);
        AssertLocalTime(times.Dhuhr, 12, 5);
        AssertLocalTime(times.Asr, 14, 57);
        AssertLocalTime(times.Maghrib, 17, 17);
        AssertLocalTime(times.Isha, 18, 38);
    }

    [Theory]
    [InlineData("Asia/Riyadh", 24.7136d, 46.6753d, CalculationMethod.UmmAlQura)]
    [InlineData("Africa/Cairo", 30.0444d, 31.2357d, CalculationMethod.Egyptian)]
    [InlineData("Europe/London", 51.5074d, -0.1278d, CalculationMethod.MuslimWorldLeague)]
    [InlineData("America/New_York", 40.7128d, -74.0060d, CalculationMethod.Isna)]
    [InlineData("Asia/Karachi", 24.8607d, 67.0011d, CalculationMethod.Karachi)]
    public void A_day_is_always_in_order_wherever_it_is_calculated(
        string timeZoneId,
        double latitude,
        double longitude,
        CalculationMethod method)
    {
        var coordinates = new Coordinates(latitude, longitude);

        // A full year, so every season and every daylight-saving transition is covered.
        for (var date = new DateOnly(2026, 1, 1); date.Year == 2026; date = date.AddDays(1))
        {
            PrayerTimes times = Engine.Calculate(coordinates, date, Settings(method, timeZoneId));

            times.Fajr.Should().BeBefore(times.Sunrise, $"{date:O} in {timeZoneId}");
            times.Sunrise.Should().BeBefore(times.Dhuhr, $"{date:O} in {timeZoneId}");
            times.Dhuhr.Should().BeBefore(times.Asr, $"{date:O} in {timeZoneId}");
            times.Asr.Should().BeBefore(times.Maghrib, $"{date:O} in {timeZoneId}");
            times.Maghrib.Should().BeBefore(times.Isha, $"{date:O} in {timeZoneId}");
        }
    }

    [Fact]
    public void Every_time_is_reported_on_the_requested_local_date()
    {
        var date = new DateOnly(2026, 6, 15);

        PrayerTimes times = Engine.Calculate(Riyadh, date, Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));

        foreach (PrayerType type in PrayerTimes.Order)
        {
            DateOnly.FromDateTime(times[type].DateTime).Should().Be(date);
        }
    }

    [Fact]
    public void The_Hanafi_Asr_falls_later_than_the_Shafi_Asr()
    {
        var date = new DateOnly(2026, 9, 29);

        PrayerTimes shafi = Engine.Calculate(Riyadh, date, Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh", Madhab.Shafi));
        PrayerTimes hanafi = Engine.Calculate(Riyadh, date, Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh", Madhab.Hanafi));

        hanafi.Asr.Should().BeAfter(shafi.Asr);
        hanafi.Asr.Should().BeBefore(hanafi.Maghrib);

        // The madhab affects nothing else.
        hanafi.Fajr.Should().Be(shafi.Fajr);
        hanafi.Dhuhr.Should().Be(shafi.Dhuhr);
        hanafi.Maghrib.Should().Be(shafi.Maghrib);
        hanafi.Isha.Should().Be(shafi.Isha);
    }

    [Fact]
    public void A_shallower_Fajr_angle_produces_a_later_Fajr()
    {
        var date = new DateOnly(2026, 9, 29);

        PrayerTimes egyptian = Engine.Calculate(Cairo, date, Settings(CalculationMethod.Egyptian, "Africa/Cairo"));
        PrayerTimes league = Engine.Calculate(Cairo, date, Settings(CalculationMethod.MuslimWorldLeague, "Africa/Cairo"));
        PrayerTimes isna = Engine.Calculate(Cairo, date, Settings(CalculationMethod.Isna, "Africa/Cairo"));

        // 19.5 degrees, then 18, then 15.
        egyptian.Fajr.Should().BeBefore(league.Fajr);
        league.Fajr.Should().BeBefore(isna.Fajr);
    }

    [Fact]
    public void An_interval_based_Isha_is_exactly_that_interval_after_Maghrib()
    {
        PrayerTimes times = Engine.Calculate(
            Riyadh,
            new DateOnly(2026, 9, 29),
            Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));

        (times.Isha - times.Maghrib).Should().Be(TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void Sunrise_and_Maghrib_are_symmetrical_around_Dhuhr()
    {
        PrayerTimes times = Engine.Calculate(
            Riyadh,
            new DateOnly(2026, 9, 29),
            Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));

        TimeSpan morning = times.Dhuhr - times.Sunrise;
        TimeSpan afternoon = times.Maghrib - times.Dhuhr;

        (morning - afternoon).Duration().Should().BeLessThan(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void Manual_adjustments_shift_only_the_prayer_they_name()
    {
        var date = new DateOnly(2026, 9, 29);
        PrayerCalculationSettings baseline = Settings(CalculationMethod.MuslimWorldLeague, "Asia/Riyadh");

        PrayerTimes unadjusted = Engine.Calculate(Riyadh, date, baseline);
        PrayerTimes adjusted = Engine.Calculate(Riyadh, date, baseline with
        {
            Adjustments = new MinuteAdjustments(Fajr: 5, Asr: -3)
        });

        (adjusted.Fajr - unadjusted.Fajr).Should().Be(TimeSpan.FromMinutes(5));
        (adjusted.Asr - unadjusted.Asr).Should().Be(TimeSpan.FromMinutes(-3));
        adjusted.Sunrise.Should().Be(unadjusted.Sunrise);
        adjusted.Dhuhr.Should().Be(unadjusted.Dhuhr);
        adjusted.Maghrib.Should().Be(unadjusted.Maghrib);
        adjusted.Isha.Should().Be(unadjusted.Isha);
    }

    [Fact]
    public void An_adjustment_to_Maghrib_leaves_an_interval_based_Isha_alone()
    {
        // Adjustments are independent per prayer. A Maghrib correction normally reflects a local
        // announcement convention rather than a claim about sunset, so it must not silently move Isha as
        // well: a user who wants both shifted adjusts both.
        var date = new DateOnly(2026, 9, 29);
        PrayerCalculationSettings baseline = Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh");

        PrayerTimes unadjusted = Engine.Calculate(Riyadh, date, baseline);
        PrayerTimes adjusted = Engine.Calculate(Riyadh, date, baseline with
        {
            Adjustments = new MinuteAdjustments(Maghrib: 4)
        });

        (adjusted.Maghrib - unadjusted.Maghrib).Should().Be(TimeSpan.FromMinutes(4));
        adjusted.Isha.Should().Be(unadjusted.Isha);
    }

    [Fact]
    public void Times_are_reported_in_the_configured_zone_not_the_device_zone()
    {
        var date = new DateOnly(2026, 9, 29);

        PrayerTimes riyadhZone = Engine.Calculate(Riyadh, date, Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));
        PrayerTimes utcZone = Engine.Calculate(Riyadh, date, Settings(CalculationMethod.UmmAlQura, "UTC"));

        riyadhZone.Dhuhr.Offset.Should().Be(TimeSpan.FromHours(3));
        utcZone.Dhuhr.Offset.Should().Be(TimeSpan.Zero);

        // The same instant, expressed two ways.
        riyadhZone.Dhuhr.UtcDateTime.Should().Be(utcZone.Dhuhr.UtcDateTime);
    }

    [Fact]
    public void An_unknown_zone_falls_back_to_UTC_rather_than_failing()
    {
        PrayerTimes times = Engine.Calculate(
            Riyadh,
            new DateOnly(2026, 9, 29),
            Settings(CalculationMethod.UmmAlQura, "Mars/Olympus_Mons"));

        times.Dhuhr.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void A_spring_daylight_saving_change_moves_the_clock_but_not_the_sun()
    {
        // British Summer Time begins at 01:00 UTC on 29 March 2026.
        PrayerCalculationSettings settings = Settings(CalculationMethod.MuslimWorldLeague, "Europe/London");

        PrayerTimes before = Engine.Calculate(London, new DateOnly(2026, 3, 28), settings);
        PrayerTimes after = Engine.Calculate(London, new DateOnly(2026, 3, 29), settings);

        before.Dhuhr.Offset.Should().Be(TimeSpan.Zero);
        after.Dhuhr.Offset.Should().Be(TimeSpan.FromHours(1));

        // Solar noon barely moves day to day, so the local clock time must jump by about an hour.
        TimeSpan clockShift = after.Dhuhr.TimeOfDay - before.Dhuhr.TimeOfDay;
        clockShift.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void An_autumn_daylight_saving_change_is_handled_the_same_way()
    {
        // Eastern Daylight Time ends at 06:00 UTC on 1 November 2026.
        PrayerCalculationSettings settings = Settings(CalculationMethod.Isna, "America/New_York");

        PrayerTimes before = Engine.Calculate(NewYork, new DateOnly(2026, 10, 31), settings);
        PrayerTimes after = Engine.Calculate(NewYork, new DateOnly(2026, 11, 1), settings);

        before.Dhuhr.Offset.Should().Be(TimeSpan.FromHours(-4));
        after.Dhuhr.Offset.Should().Be(TimeSpan.FromHours(-5));

        TimeSpan clockShift = after.Dhuhr.TimeOfDay - before.Dhuhr.TimeOfDay;
        clockShift.Should().BeCloseTo(TimeSpan.FromHours(-1), TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void Midnight_is_the_midpoint_between_Maghrib_and_the_following_Fajr()
    {
        var date = new DateOnly(2026, 9, 29);
        PrayerCalculationSettings settings = Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh");

        PrayerTimes today = Engine.Calculate(Riyadh, date, settings);
        PrayerTimes tomorrow = Engine.Calculate(Riyadh, date.AddDays(1), settings);

        today.Midnight.Should().BeAfter(today.Isha);
        today.Midnight.Should().BeBefore(tomorrow.Fajr);

        TimeSpan firstHalf = today.Midnight - today.Maghrib;
        TimeSpan secondHalf = tomorrow.Fajr - today.Midnight;
        (firstHalf - secondHalf).Duration().Should().BeLessThan(TimeSpan.FromMinutes(2));
    }

    [Theory]
    [InlineData(HighLatitudeRule.MiddleOfTheNight)]
    [InlineData(HighLatitudeRule.SeventhOfTheNight)]
    [InlineData(HighLatitudeRule.TwilightAngle)]
    public void A_London_summer_night_stays_usable_under_every_high_latitude_rule(HighLatitudeRule rule)
    {
        // In midsummer the sun in London never drops 18 degrees below the horizon, so Fajr and Isha have
        // to come from the night-division rule instead of a twilight angle. Under these rules Isha can
        // legitimately fall after midnight, and under the middle-of-the-night rule it lands on the same
        // point as the following Fajr, so the meaningful guarantee is that the night stays bounded by
        // sunset and the next sunrise.
        var date = new DateOnly(2026, 6, 21);
        PrayerCalculationSettings settings = Settings(CalculationMethod.MuslimWorldLeague, "Europe/London")
            with { HighLatitudeRule = rule };

        PrayerTimes times = Engine.Calculate(London, date, settings);
        PrayerTimes nextDay = Engine.Calculate(London, date.AddDays(1), settings);

        times.Fajr.Should().BeBefore(times.Sunrise);
        times.Maghrib.Should().BeBefore(times.Isha);
        times.Isha.Should().BeBefore(nextDay.Sunrise);
        nextDay.Fajr.Should().BeBefore(nextDay.Sunrise);
    }

    [Theory]
    [InlineData(2026, 6, 21)]
    [InlineData(2026, 12, 21)]
    [InlineData(2026, 3, 20)]
    public void The_polar_regions_still_produce_an_ordered_timetable(int year, int month, int day)
    {
        // Above the Arctic Circle the sun may not cross the horizon at all. The nearest-latitude rule has
        // to keep the app usable rather than leaving the user with no times.
        PrayerTimes times = Engine.Calculate(
            Longyearbyen,
            new DateOnly(year, month, day),
            Settings(CalculationMethod.MuslimWorldLeague, "Europe/Oslo"));

        times.Fajr.Should().BeBefore(times.Sunrise);
        times.Sunrise.Should().BeBefore(times.Dhuhr);
        times.Dhuhr.Should().BeBefore(times.Asr);
        times.Asr.Should().BeBefore(times.Maghrib);
        times.Maghrib.Should().BeBefore(times.Isha);
    }

    [Fact]
    public void Calculation_is_deterministic()
    {
        var date = new DateOnly(2026, 9, 29);
        PrayerCalculationSettings settings = Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh");

        PrayerTimes first = Engine.Calculate(Riyadh, date, settings);
        PrayerTimes second = Engine.Calculate(Riyadh, date, settings);

        foreach (PrayerType type in PrayerTimes.Order)
        {
            second[type].Should().Be(first[type]);
        }
    }

    [Fact]
    public void Elevation_brings_sunrise_forward_and_pushes_Maghrib_back()
    {
        var date = new DateOnly(2026, 9, 29);
        PrayerCalculationSettings settings = Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh");

        PrayerTimes seaLevel = Engine.Calculate(new Coordinates(24.7136d, 46.6753d), date, settings);
        PrayerTimes mountain = Engine.Calculate(new Coordinates(24.7136d, 46.6753d, 2000d), date, settings);

        mountain.Sunrise.Should().BeBefore(seaLevel.Sunrise);
        mountain.Maghrib.Should().BeAfter(seaLevel.Maghrib);
    }

    [Fact]
    public void Every_reported_time_lands_on_a_whole_minute()
    {
        PrayerTimes times = Engine.Calculate(
            Riyadh,
            new DateOnly(2026, 9, 29),
            Settings(CalculationMethod.UmmAlQura, "Asia/Riyadh"));

        foreach (PrayerType type in PrayerTimes.Order)
        {
            times[type].Second.Should().Be(0);
            times[type].Millisecond.Should().Be(0);
        }
    }

    private static PrayerCalculationSettings Settings(
        CalculationMethod method,
        string timeZoneId,
        Madhab madhab = Madhab.Shafi) => new()
        {
            Method = method,
            Madhab = madhab,
            TimeZoneId = timeZoneId
        };

    private static void AssertLocalTime(DateTimeOffset actual, int expectedHour, int expectedMinute)
    {
        var expected = new TimeSpan(expectedHour, expectedMinute, 0);
        TimeSpan difference = (actual.TimeOfDay - expected).Duration();

        difference.Should().BeLessThanOrEqualTo(
            TimeSpan.FromMinutes(ToleranceMinutes),
            $"expected about {expectedHour:00}:{expectedMinute:00} but the engine produced {actual.TimeOfDay:hh\\:mm}");
    }
}
