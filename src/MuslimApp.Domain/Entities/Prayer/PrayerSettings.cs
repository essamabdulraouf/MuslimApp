using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Prayer;

/// <summary>
/// The user's persisted prayer configuration. A single row (<see cref="SingletonId"/>) is kept; the
/// per-prayer minute corrections are stored here rather than in a separate table so there is exactly
/// one source of truth for an adjustment.
/// </summary>
public class PrayerSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public CalculationMethod CalculationMethod { get; set; } = CalculationMethod.UmmAlQura;

    public Madhab Madhab { get; set; } = Madhab.Shafi;

    public HighLatitudeRule HighLatitudeRule { get; set; } = HighLatitudeRule.MiddleOfTheNight;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double Elevation { get; set; }

    public string? LocationName { get; set; }

    public string? TimeZoneId { get; set; }

    public int FajrAdjustment { get; set; }

    public int SunriseAdjustment { get; set; }

    public int DhuhrAdjustment { get; set; }

    public int AsrAdjustment { get; set; }

    public int MaghribAdjustment { get; set; }

    public int IshaAdjustment { get; set; }

    /// <summary>Only used when <see cref="CalculationMethod"/> is <see cref="CalculationMethod.Custom"/>.</summary>
    public double? CustomFajrAngle { get; set; }

    /// <summary>Only used when <see cref="CalculationMethod"/> is <see cref="CalculationMethod.Custom"/>.</summary>
    public double? CustomIshaAngle { get; set; }

    /// <summary>Only used when <see cref="CalculationMethod"/> is <see cref="CalculationMethod.Custom"/>.</summary>
    public int? CustomIshaIntervalMinutes { get; set; }

    /// <summary>False until the user has picked a city or granted a location fix.</summary>
    public bool HasLocation { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
