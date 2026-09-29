using MuslimApp.Domain.Enums;

namespace MuslimApp.Mobile.Localization;

/// <summary>
/// Maps domain enums to resource keys.
/// </summary>
/// <remarks>
/// The domain deliberately carries no display text, so the translation of a prayer name or a calculation
/// convention belongs here rather than on the enum.
/// </remarks>
public static class DisplayNames
{
    public static string ResourceKey(PrayerType prayer) => prayer switch
    {
        PrayerType.Fajr => "Prayer_Fajr",
        PrayerType.Sunrise => "Prayer_Sunrise",
        PrayerType.Dhuhr => "Prayer_Dhuhr",
        PrayerType.Asr => "Prayer_Asr",
        PrayerType.Maghrib => "Prayer_Maghrib",
        PrayerType.Isha => "Prayer_Isha",
        _ => "Prayer_Fajr",
    };

    public static string ResourceKey(CalculationMethod method) => method switch
    {
        CalculationMethod.UmmAlQura => "CalculationMethod_UmmAlQura",
        CalculationMethod.MuslimWorldLeague => "CalculationMethod_MuslimWorldLeague",
        CalculationMethod.Egyptian => "CalculationMethod_Egyptian",
        CalculationMethod.Karachi => "CalculationMethod_Karachi",
        CalculationMethod.Isna => "CalculationMethod_Isna",
        CalculationMethod.Gulf => "CalculationMethod_Gulf",
        CalculationMethod.Custom => "CalculationMethod_Custom",
        _ => "CalculationMethod_UmmAlQura",
    };

    public static string ResourceKey(Madhab madhab) => madhab switch
    {
        Madhab.Hanafi => "Madhab_Hanafi",
        _ => "Madhab_Shafi",
    };

    public static string ResourceKey(ThemeMode theme) => theme switch
    {
        ThemeMode.Light => "Settings_Theme_Light",
        ThemeMode.Dark => "Settings_Theme_Dark",
        _ => "Settings_Theme_System",
    };

    public static string Localized(PrayerType prayer) => LocalizationResourceManager.Current[ResourceKey(prayer)];

    public static string Localized(CalculationMethod method) => LocalizationResourceManager.Current[ResourceKey(method)];

    public static string Localized(Madhab madhab) => LocalizationResourceManager.Current[ResourceKey(madhab)];

    public static string Localized(ThemeMode theme) => LocalizationResourceManager.Current[ResourceKey(theme)];
}
