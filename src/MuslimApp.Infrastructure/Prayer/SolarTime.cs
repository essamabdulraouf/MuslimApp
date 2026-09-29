using MuslimApp.Domain.Common;

namespace MuslimApp.Infrastructure.Prayer;

/// <summary>
/// One day of solar events for one position, expressed in hours of Universal Time. The surrounding
/// days are carried so every event can be interpolated rather than taken from a single snapshot.
/// </summary>
internal sealed class SolarTime
{
    private readonly SolarCoordinates _previous;
    private readonly SolarCoordinates _current;
    private readonly SolarCoordinates _next;
    private readonly double _latitude;
    private readonly double _longitude;
    private readonly double _approximateTransit;

    public SolarTime(DateOnly date, Coordinates coordinates)
        : this(date, coordinates.Latitude, coordinates.Longitude)
    {
    }

    /// <summary>
    /// Allows the latitude to differ from the caller's real position, which the engine relies on when
    /// applying the nearest-latitude rule above the polar circles.
    /// </summary>
    public SolarTime(DateOnly date, double latitude, double longitude)
    {
        double julianDay = Astronomical.JulianDay(date.Year, date.Month, date.Day);

        _previous = new SolarCoordinates(julianDay - 1d);
        _current = new SolarCoordinates(julianDay);
        _next = new SolarCoordinates(julianDay + 1d);
        _latitude = latitude;
        _longitude = longitude;

        _approximateTransit = Astronomical.ApproximateTransit(
            longitude, _current.ApparentSiderealTime, _current.RightAscension);

        Transit = Astronomical.CorrectedTransit(
            _approximateTransit,
            longitude,
            _current.ApparentSiderealTime,
            _current.RightAscension,
            _previous.RightAscension,
            _next.RightAscension);
    }

    /// <summary>Local apparent noon, in hours of Universal Time. Always defined.</summary>
    public double Transit { get; }

    public double Declination => _current.Declination;

    /// <summary>
    /// The moment the sun reaches <paramref name="altitude"/> degrees, before or after transit.
    /// Returns <see cref="double.NaN"/> when that altitude is never reached.
    /// </summary>
    public double HourAngle(double altitude, bool afterTransit) => Astronomical.CorrectedHourAngle(
        _approximateTransit,
        altitude,
        _latitude,
        _longitude,
        afterTransit,
        _current.ApparentSiderealTime,
        _current.RightAscension,
        _previous.RightAscension,
        _next.RightAscension,
        _current.Declination,
        _previous.Declination,
        _next.Declination);

    /// <summary>
    /// Asr. <paramref name="shadowLength"/> is the multiple of an object's height its shadow must reach:
    /// 1 for the Shafi/Maliki/Hanbali position, 2 for the Hanafi position.
    /// </summary>
    public double Afternoon(double shadowLength)
    {
        double tangent = Math.Abs(_latitude - _current.Declination);
        double inverse = shadowLength + AngleMath.Tan(tangent);
        double altitude = AngleMath.ToDegrees(Math.Atan(1d / inverse));

        return HourAngle(altitude, afterTransit: true);
    }
}
