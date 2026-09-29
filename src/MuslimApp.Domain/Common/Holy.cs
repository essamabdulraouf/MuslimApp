namespace MuslimApp.Domain.Common;

/// <summary>
/// Fixed reference points. These values are religious constants and must not be recalculated,
/// rounded, or "improved" without an explicit, reviewed decision.
/// </summary>
public static class Holy
{
    /// <summary>The Kaaba, Masjid al-Haram, Mecca.</summary>
    public static readonly Coordinates Kaaba = new(21.4225241d, 39.8261818d);

    /// <summary>Mean radius of the earth in kilometres, used for great-circle distances.</summary>
    public const double EarthRadiusKm = 6371.0088d;
}
