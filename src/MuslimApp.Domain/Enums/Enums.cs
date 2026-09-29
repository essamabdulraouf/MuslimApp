namespace MuslimApp.Domain.Enums;

/// <summary>
/// Established conventions for deriving Fajr and Isha twilight angles.
/// </summary>
public enum CalculationMethod
{
    UmmAlQura = 0,
    MuslimWorldLeague = 1,
    Egyptian = 2,
    Karachi = 3,
    Isna = 4,
    Gulf = 5,
    Custom = 99
}

/// <summary>
/// Juristic convention used for the Asr shadow ratio.
/// </summary>
public enum Madhab
{
    Shafi = 0,
    Hanafi = 1
}

public enum PrayerType
{
    Fajr = 0,
    Sunrise = 1,
    Dhuhr = 2,
    Asr = 3,
    Maghrib = 4,
    Isha = 5
}

/// <summary>
/// Fallback strategy for latitudes where the sun never reaches the Fajr/Isha depression angle.
/// </summary>
public enum HighLatitudeRule
{
    MiddleOfTheNight = 0,
    SeventhOfTheNight = 1,
    TwilightAngle = 2
}

public enum ThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2
}

public enum Language
{
    Arabic = 0,
    English = 1
}

/// <summary>
/// Grading of the narration backing a dhikr or dua. <see cref="Unknown"/> is the safe default so
/// unverified content is never presented as authentic.
/// </summary>
public enum AuthenticityLevel
{
    Unknown = 0,
    Weak = 1,
    Hasan = 2,
    Sahih = 3
}

public enum AzkarCategoryType
{
    Morning = 0,
    Evening = 1,
    AfterPrayer = 2,
    Sleep = 3,
    General = 4
}

public enum DuaCategoryType
{
    Quranic = 0,
    Prophetic = 1,
    Morning = 2,
    Evening = 3,
    Travel = 4,
    Family = 5,
    Health = 6,
    Provision = 7,
    Forgiveness = 8,
    General = 9
}

public enum RevelationType
{
    Meccan = 0,
    Medinan = 1
}

public enum NotificationType
{
    PrayerTime = 0,
    Adhan = 1,
    PrayerReminder = 2,
    MorningAzkar = 3,
    EveningAzkar = 4,
    AfterPrayerAzkar = 5,
    Custom = 99
}

/// <summary>
/// Where a set of coordinates came from, so the UI can explain accuracy to the user.
/// </summary>
public enum LocationSource
{
    Unknown = 0,
    Device = 1,
    LastKnown = 2,
    ManualSelection = 3,
    Stored = 4
}
