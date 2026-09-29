using FluentAssertions;
using MuslimApp.Application.Common;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Application.Tests.Common;

/// <summary>
/// Guards the built-in city list.
/// </summary>
/// <remarks>
/// These are not ceremony. A transposed digit in a latitude, or a time zone identifier that does not exist on a
/// device, produces a timetable that is quietly wrong for everyone who picks that city — and nothing else in the
/// system can detect it, because a wrong coordinate is still a valid coordinate.
/// </remarks>
public class CityCatalogTests
{
    public static TheoryData<string> AllCityKeys()
    {
        var data = new TheoryData<string>();
        foreach (CityOption city in CityCatalog.Cities)
        {
            data.Add(city.EnglishName);
        }

        return data;
    }

    private static CityOption Find(string englishName) =>
        CityCatalog.Cities.Single(city => city.EnglishName == englishName);

    [Fact]
    public void The_catalog_is_not_empty()
    {
        CityCatalog.Cities.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(AllCityKeys))]
    public void Every_city_has_coordinates_the_domain_accepts(string englishName)
    {
        CityOption city = Find(englishName);

        Coordinates.TryCreate(city.Latitude, city.Longitude, city.Elevation, out Coordinates coordinates)
            .Should().BeTrue("{0} must have coordinates inside the valid range", englishName);

        coordinates.Latitude.Should().Be(city.Latitude);
        coordinates.Longitude.Should().Be(city.Longitude);
    }

    [Theory]
    [MemberData(nameof(AllCityKeys))]
    public void Every_city_has_a_name_in_both_languages(string englishName)
    {
        CityOption city = Find(englishName);

        city.ArabicName.Should().NotBeNullOrWhiteSpace();
        city.EnglishName.Should().NotBeNullOrWhiteSpace();
        city.DisplayName(arabic: true).Should().Be(city.ArabicName);
        city.DisplayName(arabic: false).Should().Be(city.EnglishName);
    }

    [Theory]
    [MemberData(nameof(AllCityKeys))]
    public void Every_city_names_a_time_zone_this_runtime_can_resolve(string englishName)
    {
        CityOption city = Find(englishName);

        Action resolve = () => TimeZoneInfo.FindSystemTimeZoneById(city.TimeZoneId);

        resolve.Should().NotThrow("{0} declares time zone {1}", englishName, city.TimeZoneId);
    }

    [Theory]
    [MemberData(nameof(AllCityKeys))]
    public void Every_city_has_a_plausible_elevation(string englishName)
    {
        CityOption city = Find(englishName);

        // The Dead Sea shore is about -430 m and the highest inhabited places are near 5,100 m. Anything
        // outside that is a units mistake rather than a city.
        city.Elevation.Should().BeInRange(-500d, 5500d);
    }

    [Fact]
    public void No_city_appears_twice()
    {
        CityCatalog.Cities.Select(city => city.EnglishName).Should().OnlyHaveUniqueItems();
        CityCatalog.Cities.Select(city => city.ArabicName).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Makkah_sits_on_the_Kaaba()
    {
        CityOption makkah = Find("Makkah");

        // The city entry and the Kaaba constant are separate values that must not drift apart, since Makkah is
        // the one place where the two are expected to coincide.
        makkah.Latitude.Should().BeApproximately(Holy.Kaaba.Latitude, 0.01d);
        makkah.Longitude.Should().BeApproximately(Holy.Kaaba.Longitude, 0.01d);
    }

    [Theory]
    [InlineData("Makkah", CalculationMethod.UmmAlQura)]
    [InlineData("Cairo", CalculationMethod.Egyptian)]
    [InlineData("Karachi", CalculationMethod.Karachi)]
    [InlineData("New York", CalculationMethod.Isna)]
    [InlineData("Dubai", CalculationMethod.Gulf)]
    [InlineData("London", CalculationMethod.MuslimWorldLeague)]
    public void A_city_suggests_the_convention_published_for_its_region(string englishName, CalculationMethod expected)
    {
        Find(englishName).SuggestedMethod.Should().Be(expected);
    }

    [Fact]
    public void No_city_suggests_the_custom_method()
    {
        // Custom means "the user supplied their own angles". It can never be a default.
        CityCatalog.Cities.Should().NotContain(city => city.SuggestedMethod == CalculationMethod.Custom);
    }

    [Fact]
    public void A_selected_city_becomes_a_manual_location_carrying_its_zone()
    {
        CityOption riyadh = Find("Riyadh");
        var now = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(3));

        LocationResult result = riyadh.ToLocationResult(now, arabic: true);

        result.Source.Should().Be(LocationSource.ManualSelection);
        result.RetrievedAt.Should().Be(now);
        result.TimeZoneId.Should().Be("Asia/Riyadh");
        result.LocationName.Should().Be("الرياض");

        // No accuracy is claimed for a city-centre coordinate, because none was measured.
        result.AccuracyMetres.Should().BeNull();
    }

    [Fact]
    public void Searching_without_a_term_returns_everything()
    {
        CityCatalog.Search(null, arabic: true).Should().HaveCount(CityCatalog.Cities.Count);
        CityCatalog.Search("   ", arabic: true).Should().HaveCount(CityCatalog.Cities.Count);
    }

    [Fact]
    public void Searching_matches_either_script_regardless_of_the_interface_language()
    {
        CityCatalog.Search("Riyadh", arabic: true).Should().ContainSingle()
            .Which.EnglishName.Should().Be("Riyadh");

        CityCatalog.Search("الرياض", arabic: false).Should().ContainSingle()
            .Which.EnglishName.Should().Be("Riyadh");
    }

    [Fact]
    public void Searching_ignores_case_and_surrounding_space()
    {
        CityCatalog.Search("  cAiRo  ", arabic: false).Should().ContainSingle()
            .Which.EnglishName.Should().Be("Cairo");
    }

    [Fact]
    public void An_unknown_term_returns_nothing_rather_than_everything()
    {
        CityCatalog.Search("Atlantis", arabic: false).Should().BeEmpty();
    }
}
