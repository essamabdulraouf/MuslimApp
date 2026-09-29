using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Tests.Prayer;

/// <summary>
/// These tests pin the published conventions. They are not testing arithmetic: they exist so that a
/// change to any angle has to be deliberate, because silently altering one shifts every user's timetable.
/// </summary>
public class CalculationMethodParametersTests
{
    [Theory]
    [InlineData(CalculationMethod.UmmAlQura, 18.5d, null, 90)]
    [InlineData(CalculationMethod.MuslimWorldLeague, 18d, 17d, null)]
    [InlineData(CalculationMethod.Egyptian, 19.5d, 17.5d, null)]
    [InlineData(CalculationMethod.Karachi, 18d, 18d, null)]
    [InlineData(CalculationMethod.Isna, 15d, 15d, null)]
    [InlineData(CalculationMethod.Gulf, 19.5d, null, 90)]
    public void Published_conventions_have_their_documented_parameters(
        CalculationMethod method,
        double expectedFajrAngle,
        double? expectedIshaAngle,
        int? expectedIshaInterval)
    {
        CalculationMethodParameters parameters = CalculationMethodParameters.For(method);

        parameters.FajrAngle.Should().Be(expectedFajrAngle);
        parameters.IshaAngle.Should().Be(expectedIshaAngle);
        parameters.IshaIntervalMinutes.Should().Be(expectedIshaInterval);
    }

    [Fact]
    public void Every_method_defines_either_an_Isha_angle_or_an_Isha_interval()
    {
        foreach (CalculationMethodParameters parameters in CalculationMethodParameters.All)
        {
            bool hasExactlyOne = parameters.IshaAngle.HasValue ^ parameters.IshaIntervalMinutes.HasValue;
            hasExactlyOne.Should().BeTrue($"{parameters.Method} must define exactly one Isha rule");
        }
    }

    [Fact]
    public void Custom_without_a_Fajr_angle_is_rejected()
    {
        var settings = new PrayerCalculationSettings { Method = CalculationMethod.Custom, CustomIshaAngle = 17d };

        Action act = () => CalculationMethodParameters.Resolve(settings);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Custom_without_any_Isha_rule_is_rejected()
    {
        var settings = new PrayerCalculationSettings { Method = CalculationMethod.Custom, CustomFajrAngle = 18d };

        Action act = () => CalculationMethodParameters.Resolve(settings);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Custom_angles_are_used_as_given()
    {
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.Custom,
            CustomFajrAngle = 16.5d,
            CustomIshaAngle = 14.5d
        };

        CalculationMethodParameters parameters = CalculationMethodParameters.Resolve(settings);

        parameters.FajrAngle.Should().Be(16.5d);
        parameters.IshaAngle.Should().Be(14.5d);
        parameters.IshaIntervalMinutes.Should().BeNull();
    }

    [Fact]
    public void Overriding_a_published_method_keeps_its_other_parameters()
    {
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            CustomFajrAngle = 19d
        };

        CalculationMethodParameters parameters = CalculationMethodParameters.Resolve(settings);

        parameters.FajrAngle.Should().Be(19d);
        parameters.IshaAngle.Should().Be(17d, "the Isha convention was not overridden");
    }

    [Fact]
    public void An_Isha_interval_override_replaces_the_angle()
    {
        var settings = new PrayerCalculationSettings
        {
            Method = CalculationMethod.MuslimWorldLeague,
            CustomIshaIntervalMinutes = 75
        };

        CalculationMethodParameters parameters = CalculationMethodParameters.Resolve(settings);

        parameters.IshaIntervalMinutes.Should().Be(75);
        parameters.IshaAngle.Should().BeNull("an interval and an angle cannot both apply");
    }

    [Fact]
    public void An_unpublished_method_cannot_be_resolved_without_parameters()
    {
        Action act = () => CalculationMethodParameters.For(CalculationMethod.Custom);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
