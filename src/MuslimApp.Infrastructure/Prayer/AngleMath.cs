namespace MuslimApp.Infrastructure.Prayer;

/// <summary>
/// Degree-based trigonometry helpers. The astronomical formulae are all published in degrees, so
/// keeping the conversion in one place avoids the radian mistakes that silently shift a timetable.
/// </summary>
internal static class AngleMath
{
    public static double ToRadians(double degrees) => degrees * Math.PI / 180d;

    public static double ToDegrees(double radians) => radians * 180d / Math.PI;

    public static double Sin(double degrees) => Math.Sin(ToRadians(degrees));

    public static double Cos(double degrees) => Math.Cos(ToRadians(degrees));

    public static double Tan(double degrees) => Math.Tan(ToRadians(degrees));

    /// <summary>Wraps an angle into [0, 360).</summary>
    public static double Unwind(double angle) => NormalizeWithBound(angle, 360d);

    public static double NormalizeWithBound(double value, double max) => value - (max * Math.Floor(value / max));

    /// <summary>Shifts an angle into [-180, 180], used when a transit correction straddles 0h.</summary>
    public static double QuadrantShift(double angle) =>
        angle is >= -180d and <= 180d ? angle : angle - (360d * Math.Round(angle / 360d));

    /// <summary>Three-point interpolation (Meeus, chapter 3).</summary>
    public static double Interpolate(double y2, double y1, double y3, double factor)
    {
        double a = y2 - y1;
        double b = y3 - y2;
        double c = b - a;
        return y2 + (factor / 2d * (a + b + (factor * c)));
    }

    /// <summary>
    /// Three-point interpolation for angles, where the differences must be unwound so a value crossing
    /// 360 degrees does not produce a nonsense slope.
    /// </summary>
    public static double InterpolateAngles(double y2, double y1, double y3, double factor)
    {
        double a = Unwind(y2 - y1);
        double b = Unwind(y3 - y2);
        double c = b - a;
        return y2 + (factor / 2d * (a + b + (factor * c)));
    }
}
