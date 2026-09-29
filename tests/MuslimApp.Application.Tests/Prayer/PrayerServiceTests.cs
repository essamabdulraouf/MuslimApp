using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MuslimApp.Application.DTOs;
using MuslimApp.Application.Features.Prayer;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Enums;
using MuslimApp.Domain.Interfaces;
using MuslimApp.Infrastructure.Prayer;

namespace MuslimApp.Application.Tests.Prayer;

/// <summary>
/// These use the real calculation engine and mock only the boundaries the service owns: settings,
/// the timetable cache and the clock. Mocking the engine would test the mock, not the behaviour users
/// depend on.
/// </summary>
public class PrayerServiceTests
{
    private static readonly TimeZoneInfo RiyadhZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Riyadh");

    private readonly Mock<ISettingsRepository> _settings = new(MockBehavior.Strict);
    private readonly Mock<IPrayerTimesRepository> _cache = new(MockBehavior.Loose);
    private readonly IPrayerCalculationService _engine = new PrayerCalculationEngine();

    [Fact]
    public async Task Without_a_location_the_result_says_the_app_is_not_configured()
    {
        GivenSettings(new PrayerSettings { HasLocation = false });
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        Result<PrayerDayDto> today = await service.GetTodayAsync();
        Result<NextPrayerDto> next = await service.GetNextPrayerAsync();
        Result<IReadOnlyList<PrayerDayDto>> month = await service.GetMonthAsync(2026, 9);

        today.IsFailure.Should().BeTrue();
        today.Error.Should().Be(Error.NotConfigured);
        next.IsFailure.Should().BeTrue();
        month.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task A_configured_day_returns_every_entry_in_order()
    {
        GivenSettings(Riyadh());
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        Result<PrayerDayDto> result = await service.GetTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Entries.Should().HaveCount(6);
        result.Value.Entries.Select(x => x.Type).Should().Equal(PrayerTimes.Order);
        result.Value.LocationName.Should().Be("الرياض");
        result.Value.Method.Should().Be(CalculationMethod.UmmAlQura);
    }

    [Fact]
    public async Task Sunrise_is_marked_as_not_obligatory()
    {
        GivenSettings(Riyadh());
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        PrayerDayDto day = (await service.GetTodayAsync()).Value;

        day.Entries.Single(x => x.Type == PrayerType.Sunrise).IsObligatory.Should().BeFalse();
        day.Entries.Where(x => x.Type != PrayerType.Sunrise).Should().OnlyContain(x => x.IsObligatory);
    }

    [Fact]
    public async Task Entries_before_now_are_marked_as_passed_and_the_upcoming_one_as_next()
    {
        GivenSettings(Riyadh());
        // Midday in Riyadh: Fajr, Sunrise and Dhuhr have gone, Asr is next.
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        PrayerDayDto day = (await service.GetTodayAsync()).Value;

        day.Entries.Where(x => x.IsPassed).Select(x => x.Type)
            .Should().Equal(PrayerType.Fajr, PrayerType.Sunrise, PrayerType.Dhuhr);
        day.Entries.Single(x => x.IsNext).Type.Should().Be(PrayerType.Asr);
    }

    [Fact]
    public async Task Early_in_the_morning_the_next_prayer_is_todays_Fajr()
    {
        GivenSettings(Riyadh());
        PrayerService service = Build(At(2026, 9, 29, 3, 0));

        NextPrayerDto next = (await service.GetNextPrayerAsync()).Value;

        next.Type.Should().Be(PrayerType.Fajr);
        next.IsTomorrow.Should().BeFalse();
        next.TimeRemaining.Should().BePositive();
    }

    [Fact]
    public async Task After_Isha_the_next_prayer_is_tomorrows_Fajr()
    {
        GivenSettings(Riyadh());
        PrayerService service = Build(At(2026, 9, 29, 23, 30));

        NextPrayerDto next = (await service.GetNextPrayerAsync()).Value;

        next.Type.Should().Be(PrayerType.Fajr);
        next.IsTomorrow.Should().BeTrue();
        next.TimeRemaining.Should().BePositive();
        next.Time.Date.Should().Be(new DateTime(2026, 9, 30));
    }

    [Fact]
    public async Task The_countdown_matches_the_time_of_the_next_prayer()
    {
        GivenSettings(Riyadh());
        DateTimeOffset now = At(2026, 9, 29, 12, 0);
        PrayerService service = Build(now);

        NextPrayerDto next = (await service.GetNextPrayerAsync()).Value;

        (now + next.TimeRemaining).Should().Be(next.Time);
    }

    [Fact]
    public async Task The_day_shown_follows_the_configured_zone_not_the_device_zone()
    {
        GivenSettings(Riyadh());

        // 23:00 UTC on 29 September is already 02:00 on the 30th in Riyadh.
        var deviceZone = TimeZoneInfo.Utc;
        PrayerService service = Build(new DateTimeOffset(2026, 9, 29, 23, 0, 0, TimeSpan.Zero), deviceZone);

        PrayerDayDto day = (await service.GetTodayAsync()).Value;

        day.Date.Should().Be(new DateOnly(2026, 9, 30));
    }

    [Fact]
    public async Task A_month_returns_one_entry_per_day()
    {
        GivenSettings(Riyadh());
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        Result<IReadOnlyList<PrayerDayDto>> result = await service.GetMonthAsync(2026, 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(28);
        result.Value.Select(x => x.Date).Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(0, 1)]
    public async Task An_impossible_month_is_refused(int year, int month)
    {
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        Result<IReadOnlyList<PrayerDayDto>> result = await service.GetMonthAsync(year, month);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("prayer.invalid_month");
    }

    [Fact]
    public async Task A_calculated_day_is_written_to_the_cache_for_the_notification_scheduler()
    {
        GivenSettings(Riyadh());
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        await service.GetTodayAsync();

        _cache.Verify(
            x => x.UpsertAsync(It.Is<PrayerTime>(t => t.Date == new DateOnly(2026, 9, 29)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task A_failing_cache_never_stops_the_user_seeing_their_times()
    {
        GivenSettings(Riyadh());
        _cache
            .Setup(x => x.UpsertAsync(It.IsAny<PrayerTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("disk full"));

        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        Result<PrayerDayDto> result = await service.GetTodayAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Invalidating_clears_the_cache()
    {
        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        await service.InvalidateAsync();

        _cache.Verify(x => x.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task An_unknown_configured_zone_does_not_break_the_day()
    {
        PrayerSettings settings = Riyadh();
        settings.TimeZoneId = "Mars/Olympus_Mons";
        GivenSettings(settings);

        PrayerService service = Build(At(2026, 9, 29, 12, 0));

        Result<PrayerDayDto> result = await service.GetTodayAsync();

        result.IsSuccess.Should().BeTrue();
    }

    private static PrayerSettings Riyadh() => new()
    {
        Id = PrayerSettings.SingletonId,
        CalculationMethod = CalculationMethod.UmmAlQura,
        Madhab = Madhab.Shafi,
        Latitude = 24.7136d,
        Longitude = 46.6753d,
        LocationName = "الرياض",
        TimeZoneId = "Asia/Riyadh",
        HasLocation = true
    };

    private static DateTimeOffset At(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, TimeSpan.FromHours(3));

    private void GivenSettings(PrayerSettings settings) =>
        _settings
            .Setup(x => x.GetPrayerSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

    private PrayerService Build(DateTimeOffset now, TimeZoneInfo? zone = null) => new(
        _settings.Object,
        _cache.Object,
        _engine,
        new FixedDateTimeProvider(now, zone ?? RiyadhZone),
        NullLogger<PrayerService>.Instance);
}
