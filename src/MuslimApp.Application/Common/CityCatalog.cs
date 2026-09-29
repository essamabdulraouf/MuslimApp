using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Application.Common;

/// <summary>A city the user can pick without granting a location permission.</summary>
public sealed record CityOption(
    string ArabicName,
    string EnglishName,
    double Latitude,
    double Longitude,
    double Elevation,
    string TimeZoneId,
    CalculationMethod SuggestedMethod)
{
    public string DisplayName(bool arabic) => arabic ? ArabicName : EnglishName;

    public Coordinates ToCoordinates() => new(Latitude, Longitude, Elevation);

    public LocationResult ToLocationResult(DateTimeOffset retrievedAt, bool arabic) =>
        new(
            ToCoordinates(),
            LocationSource.ManualSelection,
            retrievedAt,
            AccuracyMetres: null,
            LocationName: DisplayName(arabic),
            TimeZoneId: TimeZoneId);
}

/// <summary>
/// A built-in city list so the application is fully usable with location permission denied.
/// </summary>
/// <remarks>
/// <para>
/// This is intentionally short and curated rather than a bundled gazetteer: it needs to cover the places the
/// audience actually lives, and every extra row is data that has to be verified. Coordinates are city-centre
/// values and elevations are rounded metres — both are more precise than prayer times require, since even a
/// whole degree of latitude moves Fajr by only a few minutes.
/// </para>
/// <para>
/// It lives in the application layer rather than the mobile project because a wrong number here produces a
/// wrong timetable for everyone who picks that city, which makes it something tests must be able to check.
/// </para>
/// <para>
/// The suggested method is the convention published by the authority for that region. It is only a default; the
/// user's explicit choice always wins.
/// </para>
/// </remarks>
public static class CityCatalog
{
    public static IReadOnlyList<CityOption> Cities { get; } =
    [
        new("مكة المكرمة", "Makkah", 21.4225, 39.8262, 277d, "Asia/Riyadh", CalculationMethod.UmmAlQura),
        new("المدينة المنورة", "Madinah", 24.4686, 39.6142, 608d, "Asia/Riyadh", CalculationMethod.UmmAlQura),
        new("الرياض", "Riyadh", 24.7136, 46.6753, 612d, "Asia/Riyadh", CalculationMethod.UmmAlQura),
        new("جدة", "Jeddah", 21.4858, 39.1925, 12d, "Asia/Riyadh", CalculationMethod.UmmAlQura),
        new("الدمام", "Dammam", 26.4207, 50.0888, 8d, "Asia/Riyadh", CalculationMethod.UmmAlQura),
        new("أبو ظبي", "Abu Dhabi", 24.4539, 54.3773, 5d, "Asia/Dubai", CalculationMethod.Gulf),
        new("دبي", "Dubai", 25.2048, 55.2708, 5d, "Asia/Dubai", CalculationMethod.Gulf),
        new("الدوحة", "Doha", 25.2854, 51.5310, 10d, "Asia/Qatar", CalculationMethod.Gulf),
        new("الكويت", "Kuwait City", 29.3759, 47.9774, 6d, "Asia/Kuwait", CalculationMethod.Gulf),
        new("المنامة", "Manama", 26.2285, 50.5860, 8d, "Asia/Bahrain", CalculationMethod.Gulf),
        new("مسقط", "Muscat", 23.5880, 58.3829, 15d, "Asia/Muscat", CalculationMethod.Gulf),
        new("صنعاء", "Sanaa", 15.3694, 44.1910, 2250d, "Asia/Aden", CalculationMethod.UmmAlQura),
        new("القاهرة", "Cairo", 30.0444, 31.2357, 23d, "Africa/Cairo", CalculationMethod.Egyptian),
        new("الإسكندرية", "Alexandria", 31.2001, 29.9187, 5d, "Africa/Cairo", CalculationMethod.Egyptian),
        new("الخرطوم", "Khartoum", 15.5007, 32.5599, 385d, "Africa/Khartoum", CalculationMethod.Egyptian),
        new("عمّان", "Amman", 31.9454, 35.9284, 780d, "Asia/Amman", CalculationMethod.MuslimWorldLeague),
        new("القدس", "Jerusalem", 31.7683, 35.2137, 754d, "Asia/Jerusalem", CalculationMethod.MuslimWorldLeague),
        new("بيروت", "Beirut", 33.8938, 35.5018, 56d, "Asia/Beirut", CalculationMethod.MuslimWorldLeague),
        new("دمشق", "Damascus", 33.5138, 36.2765, 680d, "Asia/Damascus", CalculationMethod.MuslimWorldLeague),
        new("بغداد", "Baghdad", 33.3152, 44.3661, 34d, "Asia/Baghdad", CalculationMethod.MuslimWorldLeague),
        new("الرباط", "Rabat", 34.0209, -6.8416, 46d, "Africa/Casablanca", CalculationMethod.MuslimWorldLeague),
        new("الجزائر", "Algiers", 36.7538, 3.0588, 24d, "Africa/Algiers", CalculationMethod.MuslimWorldLeague),
        new("تونس", "Tunis", 36.8065, 10.1815, 4d, "Africa/Tunis", CalculationMethod.MuslimWorldLeague),
        new("إستانبول", "Istanbul", 41.0082, 28.9784, 39d, "Europe/Istanbul", CalculationMethod.MuslimWorldLeague),
        new("كراتشي", "Karachi", 24.8607, 67.0011, 8d, "Asia/Karachi", CalculationMethod.Karachi),
        new("لاهور", "Lahore", 31.5204, 74.3587, 217d, "Asia/Karachi", CalculationMethod.Karachi),
        new("جاكرتا", "Jakarta", -6.2088, 106.8456, 8d, "Asia/Jakarta", CalculationMethod.MuslimWorldLeague),
        new("كوالالمبور", "Kuala Lumpur", 3.1390, 101.6869, 56d, "Asia/Kuala_Lumpur", CalculationMethod.MuslimWorldLeague),
        new("لندن", "London", 51.5072, -0.1276, 11d, "Europe/London", CalculationMethod.MuslimWorldLeague),
        new("باريس", "Paris", 48.8566, 2.3522, 35d, "Europe/Paris", CalculationMethod.MuslimWorldLeague),
        new("برلين", "Berlin", 52.5200, 13.4050, 34d, "Europe/Berlin", CalculationMethod.MuslimWorldLeague),
        new("نيويورك", "New York", 40.7128, -74.0060, 10d, "America/New_York", CalculationMethod.Isna),
        new("شيكاغو", "Chicago", 41.8781, -87.6298, 182d, "America/Chicago", CalculationMethod.Isna),
        new("تورونتو", "Toronto", 43.6532, -79.3832, 76d, "America/Toronto", CalculationMethod.Isna),
    ];

    /// <summary>
    /// Filters by either name.
    /// </summary>
    /// <remarks>
    /// Both scripts are searched regardless of the interface language, because a user reading the Arabic
    /// interface may still type "Riyadh" on a Latin keyboard.
    /// </remarks>
    public static IEnumerable<CityOption> Search(string? term, bool arabic)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Cities;
        }

        string trimmed = term.Trim();

        return Cities
            .Where(city =>
                city.ArabicName.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
                city.EnglishName.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(city => city.DisplayName(arabic), StringComparer.CurrentCulture);
    }
}
