using MuslimApp.Domain.Common;
using MuslimApp.Infrastructure.Qibla;

namespace MuslimApp.Infrastructure.Tests.Qibla;

public class QiblaCalculationServiceTests
{
    /// <summary>
    /// Bearings are initial great-circle bearings from true north, matching the values published for
    /// these cities. One degree of tolerance absorbs the small differences between published Kaaba
    /// coordinates.
    /// </summary>
    [Theory]
    [InlineData(24.7136d, 46.6753d, 243.8d)]   // Riyadh
    [InlineData(30.0444d, 31.2357d, 136.1d)]   // Cairo
    [InlineData(51.5074d, -0.1278d, 118.9d)]   // London
    [InlineData(40.7128d, -74.0060d, 58.5d)]   // New York
    [InlineData(-6.2088d, 106.8456d, 295.1d)]  // Jakarta
    [InlineData(41.0082d, 28.9784d, 151.6d)]   // Istanbul
    public void Known_cities_produce_their_published_bearing(double latitude, double longitude, double expectedBearing)
    {
        double bearing = QiblaCalculationService.BearingFromTrueNorth(new Coordinates(latitude, longitude));

        bearing.Should().BeApproximately(expectedBearing, 1d);
    }

    [Fact]
    public void A_bearing_is_always_a_compass_bearing()
    {
        for (int latitude = -85; latitude <= 85; latitude += 5)
        {
            for (int longitude = -180; longitude <= 180; longitude += 15)
            {
                double bearing = QiblaCalculationService.BearingFromTrueNorth(new Coordinates(latitude, longitude));

                bearing.Should().BeGreaterThanOrEqualTo(0d).And.BeLessThan(360d);
                double.IsNaN(bearing).Should().BeFalse($"at {latitude}, {longitude}");
            }
        }
    }

    [Fact]
    public void Due_north_of_the_Kaaba_points_south()
    {
        double bearing = QiblaCalculationService.BearingFromTrueNorth(
            new Coordinates(Holy.Kaaba.Latitude + 10d, Holy.Kaaba.Longitude));

        bearing.Should().BeApproximately(180d, 0.01d);
    }

    [Fact]
    public void Due_south_of_the_Kaaba_points_north()
    {
        double bearing = QiblaCalculationService.BearingFromTrueNorth(
            new Coordinates(Holy.Kaaba.Latitude - 10d, Holy.Kaaba.Longitude));

        bearing.Should().BeApproximately(0d, 0.01d);
    }

    [Theory]
    [InlineData(24.7136d, 46.6753d, 790d)]      // Riyadh
    [InlineData(30.0444d, 31.2357d, 1266d)]     // Cairo
    [InlineData(51.5074d, -0.1278d, 4780d)]     // London
    [InlineData(40.7128d, -74.0060d, 10318d)]   // New York
    public void Distances_match_the_published_great_circle_values(double latitude, double longitude, double expectedKm)
    {
        double distance = QiblaCalculationService.DistanceToKaabaKm(new Coordinates(latitude, longitude));

        distance.Should().BeApproximately(expectedKm, 25d);
    }

    [Fact]
    public void The_distance_from_the_Kaaba_to_itself_is_zero()
    {
        QiblaCalculationService.DistanceToKaabaKm(Holy.Kaaba).Should().BeApproximately(0d, 0.001d);
    }

    [Fact]
    public async Task The_async_entry_point_returns_the_same_result()
    {
        var coordinates = new Coordinates(24.7136d, 46.6753d);
        var service = new QiblaCalculationService();

        QiblaResult result = await service.CalculateAsync(coordinates);

        result.Coordinates.Should().Be(coordinates);
        result.BearingFromTrueNorth.Should().Be(QiblaCalculationService.BearingFromTrueNorth(coordinates));
        result.DistanceToKaabaKm.Should().Be(QiblaCalculationService.DistanceToKaabaKm(coordinates));
    }
}
