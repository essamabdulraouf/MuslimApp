using MuslimApp.Application.DTOs;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Application.Mappers;

public static class PrayerMappers
{
    /// <summary>Projects persisted settings onto the pure inputs the calculation engine accepts.</summary>
    public static PrayerCalculationSettings ToCalculationSettings(this PrayerSettings settings) => new()
    {
        Method = settings.CalculationMethod,
        Madhab = settings.Madhab,
        HighLatitudeRule = settings.HighLatitudeRule,
        TimeZoneId = settings.TimeZoneId,
        CustomFajrAngle = settings.CustomFajrAngle,
        CustomIshaAngle = settings.CustomIshaAngle,
        CustomIshaIntervalMinutes = settings.CustomIshaIntervalMinutes,
        Adjustments = new MinuteAdjustments(
            settings.FajrAdjustment,
            settings.SunriseAdjustment,
            settings.DhuhrAdjustment,
            settings.AsrAdjustment,
            settings.MaghribAdjustment,
            settings.IshaAdjustment)
    };

    public static Coordinates ToCoordinates(this PrayerSettings settings) =>
        new(settings.Latitude, settings.Longitude, settings.Elevation);

    public static PrayerTime ToEntity(this PrayerTimes times) => new()
    {
        Date = times.Date,
        Fajr = times.Fajr,
        Sunrise = times.Sunrise,
        Dhuhr = times.Dhuhr,
        Asr = times.Asr,
        Maghrib = times.Maghrib,
        Isha = times.Isha
    };

    public static PrayerDayDto ToDto(this PrayerTimes times, DateTimeOffset now, PrayerSettings settings)
    {
        (PrayerType Type, DateTimeOffset Time)? next = times.GetNext(now);

        var entries = new List<PrayerEntryDto>(PrayerTimes.Order.Count);
        foreach (PrayerType type in PrayerTimes.Order)
        {
            DateTimeOffset time = times[type];
            entries.Add(new PrayerEntryDto(
                type,
                time,
                IsPassed: time <= now,
                IsNext: next?.Type == type,
                IsObligatory: type != PrayerType.Sunrise));
        }

        return new PrayerDayDto(
            times.Date,
            settings.LocationName,
            settings.CalculationMethod,
            settings.Madhab,
            entries,
            times.Midnight);
    }

    public static CalculationMethodOptionDto ToOptionDto(this CalculationMethodParameters parameters) => new(
        parameters.Method,
        $"CalculationMethod_{parameters.Method}",
        parameters.FajrAngle,
        parameters.IshaAngle,
        parameters.IshaIntervalMinutes);
}
