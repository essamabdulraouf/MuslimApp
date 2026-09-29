using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Common;
using MuslimApp.Application.DTOs;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Enums;
using MuslimApp.Mobile.Localization;
using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.Services;
using MuslimApp.Mobile.ViewModels.Items;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>
/// Settings: language, appearance, and the three inputs that decide a prayer timetable.
/// </summary>
/// <remarks>
/// Every change is written immediately rather than collected behind a save button. There is no multi-field form
/// here to validate, and a setting that visibly takes effect at once is easier to trust than one that might not
/// have been saved.
/// </remarks>
public sealed partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IAppPresentation _presentation;
    private readonly INavigationService _navigation;
    private readonly ILogger<SettingsViewModel> _logger;

    public SettingsViewModel(
        ISettingsService settingsService,
        IAppPresentation presentation,
        INavigationService navigation,
        ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _presentation = presentation;
        _navigation = navigation;
        _logger = logger;

        TitleKey = "Settings_Title";
    }

    public ObservableCollection<MethodOptionItem> Methods { get; } = [];

    [ObservableProperty]
    private Language _language = Language.Arabic;

    [ObservableProperty]
    private ThemeMode _theme = ThemeMode.System;

    [ObservableProperty]
    private Madhab _madhab = Madhab.Shafi;

    [ObservableProperty]
    private string _locationName = string.Empty;

    public bool IsArabic => Language == Language.Arabic;

    public bool IsEnglish => Language == Language.English;

    public bool IsThemeSystem => Theme == ThemeMode.System;

    public bool IsThemeLight => Theme == ThemeMode.Light;

    public bool IsThemeDark => Theme == ThemeMode.Dark;

    public bool IsShafi => Madhab == Madhab.Shafi;

    public bool IsHanafi => Madhab == Madhab.Hanafi;

    public override async Task OnAppearingAsync()
    {
        BeginLoading();

        try
        {
            AppSettings app = await _settingsService.GetAppSettingsAsync();
            PrayerSettings prayer = await _settingsService.GetPrayerSettingsAsync();

            Language = app.Language;
            Theme = app.Theme;
            Madhab = prayer.Madhab;

            LocationName = prayer.HasLocation && !string.IsNullOrWhiteSpace(prayer.LocationName)
                ? prayer.LocationName!
                : LocalizationResourceManager.Current["Settings_LocationUnset"];

            LoadMethods(prayer.CalculationMethod);

            State = OperationState.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Loading settings failed.");
            SetFailure(OperationState.Error, "Common_Error");
        }
    }

    [RelayCommand]
    private async Task SetLanguageAsync(string? value)
    {
        if (!Enum.TryParse(value, out Language language) || language == Language)
        {
            return;
        }

        Language = language;
        await _settingsService.SetLanguageAsync(language);
        _presentation.ApplyLanguage(language);

        // The list rows carry already-translated text, so they are rebuilt rather than left stale.
        RefreshTranslatedLists();
    }

    [RelayCommand]
    private async Task SetThemeAsync(string? value)
    {
        if (!Enum.TryParse(value, out ThemeMode theme) || theme == Theme)
        {
            return;
        }

        Theme = theme;
        await _settingsService.SetThemeAsync(theme);
        _presentation.ApplyTheme(theme);
    }

    [RelayCommand]
    private async Task SetMadhabAsync(string? value)
    {
        if (!Enum.TryParse(value, out Madhab madhab) || madhab == Madhab)
        {
            return;
        }

        Madhab = madhab;
        await _settingsService.SetMadhabAsync(madhab);
    }

    [RelayCommand]
    private async Task SelectMethodAsync(MethodOptionItem? item)
    {
        if (item is null || item.IsSelected)
        {
            return;
        }

        foreach (MethodOptionItem option in Methods)
        {
            option.IsSelected = option.Method == item.Method;
        }

        await _settingsService.SetCalculationMethodAsync(item.Method);
    }

    [RelayCommand]
    private Task OpenLocationAsync() => _navigation.GoToAsync(Routes.Location);

    private void LoadMethods(CalculationMethod current)
    {
        Methods.Clear();
        foreach (CalculationMethodOptionDto option in CalculationMethodOptions.All)
        {
            // Custom needs its own editor for the angles; until that exists it is not offered as a choice.
            if (option.Method == CalculationMethod.Custom)
            {
                continue;
            }

            Methods.Add(new MethodOptionItem(option, option.Method == current));
        }
    }

    private void RefreshTranslatedLists()
    {
        CalculationMethod current = Methods.FirstOrDefault(m => m.IsSelected)?.Method ?? CalculationMethod.UmmAlQura;
        LoadMethods(current);
    }

    partial void OnLanguageChanged(Language value)
    {
        OnPropertyChanged(nameof(IsArabic));
        OnPropertyChanged(nameof(IsEnglish));
    }

    partial void OnThemeChanged(ThemeMode value)
    {
        OnPropertyChanged(nameof(IsThemeSystem));
        OnPropertyChanged(nameof(IsThemeLight));
        OnPropertyChanged(nameof(IsThemeDark));
    }

    partial void OnMadhabChanged(Madhab value)
    {
        OnPropertyChanged(nameof(IsShafi));
        OnPropertyChanged(nameof(IsHanafi));
    }
}
