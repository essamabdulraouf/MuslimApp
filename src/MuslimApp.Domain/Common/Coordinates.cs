namespace MuslimApp.Domain.Common;

/// <summary>
/// A validated geographic position. Elevation is in metres above sea level and only affects
/// sunrise/sunset refraction.
/// </summary>
public readonly record struct Coordinates
{
    public Coordinates(double latitude, double longitude, double elevation = 0d)
    {
        if (double.IsNaN(latitude) || double.IsInfinity(latitude) || latitude is < -90d or > 90d)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be between -90 and 90 degrees.");
        }

        if (double.IsNaN(longitude) || double.IsInfinity(longitude) || longitude is < -180d or > 180d)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be between -180 and 180 degrees.");
        }

        if (double.IsNaN(elevation) || double.IsInfinity(elevation) || elevation < -500d)
        {
            throw new ArgumentOutOfRangeException(nameof(elevation), elevation, "Elevation must be a real value above -500 metres.");
        }

        Latitude = latitude;
        Longitude = longitude;
        Elevation = elevation;
    }

    public double Latitude { get; }

    public double Longitude { get; }

    public double Elevation { get; }

    public static bool TryCreate(double latitude, double longitude, double elevation, out Coordinates coordinates)
    {
        try
        {
            coordinates = new Coordinates(latitude, longitude, elevation);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            coordinates = default;
            return false;
        }
    }

    public override string ToString() =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Latitude:0.0000}, {Longitude:0.0000}");
}
