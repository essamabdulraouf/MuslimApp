using CommunityToolkit.Mvvm.ComponentModel;
using MuslimApp.Application.Common;
using MuslimApp.Application.DTOs;
using MuslimApp.Domain.Enums;
using MuslimApp.Mobile.Localization;

namespace MuslimApp.Mobile.ViewModels.Items;

/// <summary>
/// A calculation convention in a picker, with its angles shown.
/// </summary>
/// <remarks>
/// The angles are surfaced deliberately. A user who knows their mosque follows an 18° Fajr can recognise the
/// right entry without knowing which body publishes it, and seeing the numbers makes it clear that the app is
/// applying a published convention rather than inventing times.
/// </remarks>
public sealed partial class MethodOptionItem : ObservableObject
{
    public MethodOptionItem(CalculationMethodOptionDto option, bool isSelected)
    {
        Option = option;
        _isSelected = isSelected;
    }

    public CalculationMethodOptionDto Option { get; }

    public CalculationMethod Method => Option.Method;

    public string Name => LocalizationResourceManager.Current[Option.ResourceKey];

    /// <summary>A compact summary such as "الفجر ‎18.5°‎ · العشاء ‎90‎ د".</summary>
    public string Summary
    {
        get
        {
            string fajr = $"{Option.FajrAngle:0.##}°";

            string isha = Option.IshaAngle is not null
                ? $"{Option.IshaAngle.Value:0.##}°"
                : Option.IshaIntervalMinutes is not null
                    ? $"+{Option.IshaIntervalMinutes.Value}"
                    : "—";

            return $"{LocalizationResourceManager.Current["Prayer_Fajr"]} {fajr}  ·  {LocalizationResourceManager.Current["Prayer_Isha"]} {isha}";
        }
    }

    [ObservableProperty]
    private bool _isSelected;
}

/// <summary>A city in the manual location picker.</summary>
public sealed class CityItem(CityOption option, bool arabic)
{
    public CityOption Option { get; } = option;

    public string Name { get; } = option.DisplayName(arabic);

    /// <summary>The other language's name, so a user searching in either script recognises the row.</summary>
    public string Alternate { get; } = arabic ? option.EnglishName : option.ArabicName;
}
