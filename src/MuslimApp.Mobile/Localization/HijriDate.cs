using System.Globalization;

namespace MuslimApp.Mobile.Localization;

/// <summary>
/// Formats a Gregorian date as its Umm al-Qura Hijri equivalent.
/// </summary>
/// <remarks>
/// <para>
/// Umm al-Qura is used rather than the tabular <see cref="HijriCalendar"/> because it is the civil calendar of
/// Saudi Arabia and therefore the one the primary audience sees on official documents. The two can differ by a
/// day.
/// </para>
/// <para>
/// Month names are written out here instead of being read from culture data. ICU's Arabic month names vary by
/// platform and version, and a date shown beside prayer times should not change spelling between an Android
/// and an iOS build.
/// </para>
/// <para>
/// This is a display convenience only. It is never a basis for religious rulings: the start of a month is
/// determined by sighting, not by arithmetic, and the app makes no claim otherwise.
/// </para>
/// </remarks>
public static class HijriDate
{
    private static readonly UmAlQuraCalendar Calendar = new();

    private static readonly string[] ArabicMonths =
    [
        "محرم", "صفر", "ربيع الأول", "ربيع الآخر", "جمادى الأولى", "جمادى الآخرة",
        "رجب", "شعبان", "رمضان", "شوال", "ذو القعدة", "ذو الحجة",
    ];

    private static readonly string[] EnglishMonths =
    [
        "Muharram", "Safar", "Rabi al-Awwal", "Rabi al-Thani", "Jumada al-Ula", "Jumada al-Akhirah",
        "Rajab", "Shaban", "Ramadan", "Shawwal", "Dhu al-Qidah", "Dhu al-Hijjah",
    ];

    public static string Format(DateOnly date, bool arabic)
    {
        try
        {
            DateTime value = date.ToDateTime(TimeOnly.MinValue);

            if (value < Calendar.MinSupportedDateTime || value > Calendar.MaxSupportedDateTime)
            {
                return string.Empty;
            }

            int day = Calendar.GetDayOfMonth(value);
            int month = Calendar.GetMonth(value);
            int year = Calendar.GetYear(value);

            string monthName = arabic ? ArabicMonths[month - 1] : EnglishMonths[month - 1];
            string suffix = arabic ? "هـ" : "AH";

            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", day, monthName, year, suffix);
        }
        catch (ArgumentException)
        {
            // Outside the tabulated Umm al-Qura range. Omitting the line is better than showing a wrong date.
            return string.Empty;
        }
    }

    public static string FormatGregorian(DateOnly date, bool arabic)
    {
        CultureInfo culture = CultureInfo.GetCultureInfo(arabic ? "ar" : "en");
        return date.ToString("dd MMMM yyyy", culture);
    }
}
