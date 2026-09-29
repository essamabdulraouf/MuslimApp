namespace MuslimApp.Infrastructure.Prayer;

/// <summary>
/// Solar position and rise/set mathematics from Jean Meeus, <em>Astronomical Algorithms</em>
/// (2nd edition, chapters 7, 13, 15, 22 and 25), using the low-precision solar series that is accurate
/// to roughly one arc-minute — far beyond what a one-minute prayer timetable needs.
/// </summary>
/// <remarks>
/// Every value here is an astronomical constant from the reference. None of them are tuning
/// parameters, and none may be changed to "fix" a timetable: a wrong time is a configuration problem
/// (method, madhab, adjustment, zone), not a problem with these coefficients.
/// </remarks>
internal static class Astronomical
{
    /// <summary>Julian day for a calendar date in Universal Time (Meeus, chapter 7).</summary>
    public static double JulianDay(int year, int month, int day, double hours = 0d)
    {
        int adjustedYear = month > 2 ? year : year - 1;
        int adjustedMonth = month > 2 ? month : month + 12;
        double dayFraction = day + (hours / 24d);

        int century = (int)Math.Floor(adjustedYear / 100d);
        int gregorianCorrection = 2 - century + (int)Math.Floor(century / 4d);

        return Math.Floor(365.25d * (adjustedYear + 4716))
            + Math.Floor(30.6001d * (adjustedMonth + 1))
            + dayFraction
            + gregorianCorrection
            - 1524.5d;
    }

    /// <summary>Julian centuries since J2000.0 (Meeus, 25.1).</summary>
    public static double JulianCentury(double julianDay) => (julianDay - 2451545.0d) / 36525d;

    /// <summary>Geometric mean longitude of the sun in degrees (Meeus, 25.2).</summary>
    public static double MeanSolarLongitude(double t) =>
        AngleMath.Unwind(280.4664567d + (t * (36000.76983d + (t * 0.0003032d))));

    /// <summary>Mean longitude of the moon in degrees, needed only for nutation (Meeus, 47.1).</summary>
    public static double MeanLunarLongitude(double t) =>
        AngleMath.Unwind(218.3165d + (481267.8813d * t));

    /// <summary>Longitude of the ascending lunar node in degrees (Meeus, chapter 47).</summary>
    public static double AscendingLunarNodeLongitude(double t) =>
        AngleMath.Unwind(125.04452d
            - (1934.136261d * t)
            + (0.0020708d * t * t)
            + (t * t * t / 450000d));

    /// <summary>Mean anomaly of the sun in degrees (Meeus, 25.3).</summary>
    public static double MeanSolarAnomaly(double t) =>
        AngleMath.Unwind(357.52911d + (t * (35999.05029d - (0.0001537d * t))));

    /// <summary>Equation of the centre of the sun in degrees (Meeus, chapter 25).</summary>
    public static double SolarEquationOfTheCenter(double t, double meanAnomaly) =>
        ((1.914602d - (t * (0.004817d + (0.000014d * t)))) * AngleMath.Sin(meanAnomaly))
        + ((0.019993d - (0.000101d * t)) * AngleMath.Sin(2d * meanAnomaly))
        + (0.000289d * AngleMath.Sin(3d * meanAnomaly));

    /// <summary>Apparent longitude of the sun in degrees, corrected for nutation and aberration.</summary>
    public static double ApparentSolarLongitude(double t, double meanLongitude)
    {
        double trueLongitude = meanLongitude + SolarEquationOfTheCenter(t, MeanSolarAnomaly(t));
        double omega = 125.04d - (1934.136d * t);
        return AngleMath.Unwind(trueLongitude - 0.00569d - (0.00478d * AngleMath.Sin(omega)));
    }

    /// <summary>Mean obliquity of the ecliptic in degrees (Meeus, 22.2).</summary>
    public static double MeanObliquityOfTheEcliptic(double t) =>
        23.439291d - (t * (0.013004167d + (t * (0.0000001639d - (t * 0.0000005036d)))));

    /// <summary>Apparent obliquity of the ecliptic in degrees (Meeus, 25.8).</summary>
    public static double ApparentObliquityOfTheEcliptic(double t, double meanObliquity) =>
        meanObliquity + (0.00256d * AngleMath.Cos(125.04d - (1934.136d * t)));

    /// <summary>Mean sidereal time at Greenwich in degrees (Meeus, 12.4).</summary>
    public static double MeanSiderealTime(double t)
    {
        double julianDay = (t * 36525d) + 2451545.0d;
        double theta = 280.46061837d
            + (360.98564736629d * (julianDay - 2451545d))
            + (0.000387933d * t * t)
            - (t * t * t / 38710000d);
        return AngleMath.Unwind(theta);
    }

    /// <summary>Nutation in longitude in degrees (Meeus, chapter 22, abridged series).</summary>
    public static double NutationInLongitude(double solarLongitude, double lunarLongitude, double ascendingNode) =>
        ((-17.2d * AngleMath.Sin(ascendingNode))
            - (1.32d * AngleMath.Sin(2d * solarLongitude))
            - (0.23d * AngleMath.Sin(2d * lunarLongitude))
            + (0.21d * AngleMath.Sin(2d * ascendingNode))) / 3600d;

    /// <summary>Nutation in obliquity in degrees (Meeus, chapter 22, abridged series).</summary>
    public static double NutationInObliquity(double solarLongitude, double lunarLongitude, double ascendingNode) =>
        ((9.2d * AngleMath.Cos(ascendingNode))
            + (0.57d * AngleMath.Cos(2d * solarLongitude))
            + (0.10d * AngleMath.Cos(2d * lunarLongitude))
            - (0.09d * AngleMath.Cos(2d * ascendingNode))) / 3600d;

    /// <summary>Altitude of the sun in degrees for a given local hour angle (Meeus, 13.6).</summary>
    public static double AltitudeOfCelestialBody(double latitude, double declination, double localHourAngle) =>
        AngleMath.ToDegrees(Math.Asin(
            (AngleMath.Sin(latitude) * AngleMath.Sin(declination))
            + (AngleMath.Cos(latitude) * AngleMath.Cos(declination) * AngleMath.Cos(localHourAngle))));

    /// <summary>First estimate of the transit, as a fraction of a day (Meeus, chapter 15).</summary>
    public static double ApproximateTransit(double longitude, double siderealTime, double rightAscension)
    {
        double westLongitude = -longitude;
        return AngleMath.NormalizeWithBound((rightAscension + westLongitude - siderealTime) / 360d, 1d);
    }

    /// <summary>
    /// Solar transit (local apparent noon) in hours of Universal Time, interpolated across the
    /// surrounding days so the result is accurate near the equinoxes.
    /// </summary>
    public static double CorrectedTransit(
        double approximateTransit,
        double longitude,
        double siderealTime,
        double rightAscension,
        double previousRightAscension,
        double nextRightAscension)
    {
        double westLongitude = -longitude;
        double theta = AngleMath.Unwind(siderealTime + (360.985647d * approximateTransit));
        double alpha = AngleMath.Unwind(AngleMath.InterpolateAngles(
            rightAscension, previousRightAscension, nextRightAscension, approximateTransit));
        double hourAngle = AngleMath.QuadrantShift(theta - westLongitude - alpha);
        double correction = hourAngle / -360d;

        return (approximateTransit + correction) * 24d;
    }

    /// <summary>
    /// The moment in hours of Universal Time at which the sun reaches altitude
    /// <paramref name="targetAltitude"/>, or <see cref="double.NaN"/> when it never does on this day.
    /// </summary>
    public static double CorrectedHourAngle(
        double approximateTransit,
        double targetAltitude,
        double latitude,
        double longitude,
        bool afterTransit,
        double siderealTime,
        double rightAscension,
        double previousRightAscension,
        double nextRightAscension,
        double declination,
        double previousDeclination,
        double nextDeclination)
    {
        double westLongitude = -longitude;

        double numerator = AngleMath.Sin(targetAltitude) - (AngleMath.Sin(latitude) * AngleMath.Sin(declination));
        double denominator = AngleMath.Cos(latitude) * AngleMath.Cos(declination);
        double term = numerator / denominator;

        if (double.IsNaN(term) || term is > 1d or < -1d)
        {
            // The sun stays above or below this altitude for the whole day.
            return double.NaN;
        }

        double initialHourAngle = AngleMath.ToDegrees(Math.Acos(term));
        double estimate = afterTransit
            ? approximateTransit + (initialHourAngle / 360d)
            : approximateTransit - (initialHourAngle / 360d);

        double theta = AngleMath.Unwind(siderealTime + (360.985647d * estimate));
        double alpha = AngleMath.Unwind(AngleMath.InterpolateAngles(
            rightAscension, previousRightAscension, nextRightAscension, estimate));
        double interpolatedDeclination = AngleMath.Interpolate(
            declination, previousDeclination, nextDeclination, estimate);

        double localHourAngle = theta - westLongitude - alpha;
        double altitude = AltitudeOfCelestialBody(latitude, interpolatedDeclination, localHourAngle);

        double correction = (altitude - targetAltitude)
            / (360d * AngleMath.Cos(interpolatedDeclination) * AngleMath.Cos(latitude) * AngleMath.Sin(localHourAngle));

        return (estimate + correction) * 24d;
    }
}
