using MuslimApp.Domain.Common;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Qibla;

/// <summary>
/// Great-circle bearing and distance to the Kaaba.
/// </summary>
/// <remarks>
/// This is the geographical half of the qibla only. It returns a bearing from <em>true</em> north; a
/// compass reports magnetic north, so the presentation layer is responsible for declination and
/// calibration. Keeping the two apart is what lets this half be tested against published bearings.
/// </remarks>
public sealed class QiblaCalculationService : IQiblaService
{
    public Task<QiblaResult> CalculateAsync(Coordinates coordinates, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Calculate(coordinates));
    }

    /// <summary>
    /// Synchronous entry point. The interface is asynchronous because callers reach it through a device
    /// location lookup, but the mathematics itself never blocks.
    /// </summary>
    public static QiblaResult Calculate(Coordinates coordinates) => new(
        coordinates,
        BearingFromTrueNorth(coordinates),
        DistanceToKaabaKm(coordinates));

    /// <summary>
    /// Initial great-circle bearing in degrees clockwise from true north, in [0, 360).
    /// </summary>
    public static double BearingFromTrueNorth(Coordinates from)
    {
        double latitude = ToRadians(from.Latitude);
        double kaabaLatitude = ToRadians(Holy.Kaaba.Latitude);
        double longitudeDelta = ToRadians(Holy.Kaaba.Longitude - from.Longitude);

        double y = Math.Sin(longitudeDelta);
        double x = (Math.Cos(latitude) * Math.Tan(kaabaLatitude)) - (Math.Sin(latitude) * Math.Cos(longitudeDelta));

        double bearing = ToDegrees(Math.Atan2(y, x));
        return (bearing + 360d) % 360d;
    }

    /// <summary>Great-circle distance in kilometres, via the haversine formula.</summary>
    public static double DistanceToKaabaKm(Coordinates from)
    {
        double latitude = ToRadians(from.Latitude);
        double kaabaLatitude = ToRadians(Holy.Kaaba.Latitude);
        double latitudeDelta = kaabaLatitude - latitude;
        double longitudeDelta = ToRadians(Holy.Kaaba.Longitude - from.Longitude);

        double haversine = (Math.Sin(latitudeDelta / 2d) * Math.Sin(latitudeDelta / 2d))
            + (Math.Cos(latitude) * Math.Cos(kaabaLatitude) * Math.Sin(longitudeDelta / 2d) * Math.Sin(longitudeDelta / 2d));

        return 2d * Holy.EarthRadiusKm * Math.Asin(Math.Min(1d, Math.Sqrt(haversine)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;

    private static double ToDegrees(double radians) => radians * 180d / Math.PI;
}
