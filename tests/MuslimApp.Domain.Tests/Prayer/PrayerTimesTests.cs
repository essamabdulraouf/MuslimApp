using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Tests.Prayer;

public class PrayerTimesTests
{
    private static readonly DateOnly Date = new(2026, 9, 29);
    private static readonly TimeSpan Riyadh = TimeSpan.FromHours(3);

    private static PrayerTimes Build() => new(
        Date,
        At(4, 27),
        At(5, 44),
        At(11, 44),
        At(15, 8),
        At(17, 43),
        At(19, 13),
        At(23, 5));

    private static DateTimeOffset At(int hour, int minute) =>
        new(Date.Year, Date.Month, Date.Day, hour, minute, 0, Riyadh);

    [Fact]
    public void Indexer_returns_the_matching_entry()
    {
        PrayerTimes times = Build();

        times[PrayerType.Fajr].Should().Be(times.Fajr);
        times[PrayerType.Sunrise].Should().Be(times.Sunrise);
        times[PrayerType.Dhuhr].Should().Be(times.Dhuhr);
        times[PrayerType.Asr].Should().Be(times.Asr);
        times[PrayerType.Maghrib].Should().Be(times.Maghrib);
        times[PrayerType.Isha].Should().Be(times.Isha);
    }

    [Fact]
    public void Next_is_the_first_entry_strictly_after_the_instant()
    {
        PrayerTimes times = Build();

        times.GetNext(At(12, 0))!.Value.Type.Should().Be(PrayerType.Asr);
    }

    [Fact]
    public void An_instant_exactly_on_a_prayer_treats_it_as_current_not_next()
    {
        PrayerTimes times = Build();

        times.GetNext(At(11, 44))!.Value.Type.Should().Be(PrayerType.Asr);
        times.GetCurrent(At(11, 44))!.Value.Type.Should().Be(PrayerType.Dhuhr);
    }

    [Fact]
    public void There_is_no_next_entry_once_Isha_has_passed()
    {
        PrayerTimes times = Build();

        times.GetNext(At(20, 0)).Should().BeNull("the caller rolls over to tomorrow's Fajr");
    }

    [Fact]
    public void Before_Fajr_there_is_no_current_entry()
    {
        PrayerTimes times = Build();

        times.GetCurrent(At(3, 0)).Should().BeNull();
    }

    [Fact]
    public void Order_lists_the_day_chronologically()
    {
        PrayerTimes.Order.Should().Equal(
            PrayerType.Fajr,
            PrayerType.Sunrise,
            PrayerType.Dhuhr,
            PrayerType.Asr,
            PrayerType.Maghrib,
            PrayerType.Isha);
    }
}
