using System.Globalization;
using MuslimApp.Application.DTOs;
using MuslimApp.Mobile.Localization;

namespace MuslimApp.Mobile.ViewModels.Items;

/// <summary>
/// One timetable row, ready to bind.
/// </summary>
/// <remarks>
/// The clock string is formatted here rather than in a converter so the list has no per-row conversion work,
/// and so the 24-hour, Western-digit format used across the timetable is decided in exactly one place.
/// </remarks>
public sealed class PrayerEntryItem
{
    public PrayerEntryItem(PrayerEntryDto entry)
    {
        Type = entry.Type;
        Time = entry.Time.ToString("HH:mm", CultureInfo.InvariantCulture);
        IsPassed = entry.IsPassed;
        IsNext = entry.IsNext;
        IsObligatory = entry.IsObligatory;
        NameKey = DisplayNames.ResourceKey(entry.Type);
    }

    public Domain.Enums.PrayerType Type { get; }

    public string NameKey { get; }

    public string Name => LocalizationResourceManager.Current[NameKey];

    public string Time { get; }

    public bool IsPassed { get; }

    public bool IsNext { get; }

    /// <summary>Sunrise is a timetable landmark, not a prayer, and the list marks it as such.</summary>
    public bool IsObligatory { get; }
}
