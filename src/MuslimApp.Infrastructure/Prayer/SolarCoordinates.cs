namespace MuslimApp.Infrastructure.Prayer;

/// <summary>
/// The sun's apparent position for one Julian day, plus the sidereal time needed to turn it into a
/// local hour angle.
/// </summary>
internal readonly struct SolarCoordinates
{
    public SolarCoordinates(double julianDay)
    {
        double t = Astronomical.JulianCentury(julianDay);
        double meanSolarLongitude = Astronomical.MeanSolarLongitude(t);
        double meanLunarLongitude = Astronomical.MeanLunarLongitude(t);
        double ascendingNode = Astronomical.AscendingLunarNodeLongitude(t);
        double apparentLongitude = Astronomical.ApparentSolarLongitude(t, meanSolarLongitude);

        double meanObliquity = Astronomical.MeanObliquityOfTheEcliptic(t);
        double apparentObliquity = Astronomical.ApparentObliquityOfTheEcliptic(t, meanObliquity);

        double nutationInLongitude = Astronomical.NutationInLongitude(meanSolarLongitude, meanLunarLongitude, ascendingNode);
        double nutationInObliquity = Astronomical.NutationInObliquity(meanSolarLongitude, meanLunarLongitude, ascendingNode);

        Declination = AngleMath.ToDegrees(Math.Asin(
            AngleMath.Sin(apparentObliquity) * AngleMath.Sin(apparentLongitude)));

        RightAscension = AngleMath.Unwind(AngleMath.ToDegrees(Math.Atan2(
            AngleMath.Cos(apparentObliquity) * AngleMath.Sin(apparentLongitude),
            AngleMath.Cos(apparentLongitude))));

        ApparentSiderealTime = Astronomical.MeanSiderealTime(t)
            + (nutationInLongitude * AngleMath.Cos(meanObliquity + nutationInObliquity));
    }

    /// <summary>Apparent declination of the sun in degrees.</summary>
    public double Declination { get; }

    /// <summary>Apparent right ascension of the sun in degrees, in [0, 360).</summary>
    public double RightAscension { get; }

    /// <summary>Apparent sidereal time at Greenwich in degrees.</summary>
    public double ApparentSiderealTime { get; }
}
