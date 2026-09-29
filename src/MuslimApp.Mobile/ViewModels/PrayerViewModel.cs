using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Common;
using MuslimApp.Application.DTOs;
using MuslimApp.Domain.Common;
using MuslimApp.Mobile.Localization;
using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.ViewModels.Items;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>
/// The prayer screen: the full timetable for one day, with the day selectable.
/// </summary>
/// <remarks>
/// Day navigation recalculates rather than paging a cache, which is what makes crossing a daylight-saving
/// boundary or a month end correct without special handling. The configured convention and juristic method are
/// shown on the screen so a user comparing against their mosque can see which assumptions produced the times.
/// </remarks>
public sealed partial class PrayerViewModel : BaseViewModel, IDisposable
{
    private readonly IPrayerService _prayerService;
    private readonly ISettingsService _settingsService;
    private readonly IDateTimeProvider _clock;
    private readonly INavigationService _navigation;
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<PrayerViewModel> _logger;

    private DateOnly _date;
    private bool _disposed;

    public PrayerViewModel(
        IPrayerService prayerService,
        ISettingsService settingsService,
        IDateTimeProvider clock,
        INavigationService navigation,
        IDispatcher dispatcher,
        ILogger<PrayerViewModel> logger)
    {
        _prayerService = prayerService;
        _settingsService = settingsService;
        _clock = clock;
        _navigation = navigation;
        _dispatcher = dispatcher;
        _logger = logger;

        TitleKey = "Prayer_Title";
        _date = _clock.Today;
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    public ObservableCollection<PrayerEntryItem> Entries { get; } = [];

    [ObservableProperty]
    private string _hijriDateText = string.Empty;

    [ObservableProperty]
    private string _gregorianDateText = string.Empty;

    [ObservableProperty]
    private string _locationName = string.Empty;

    [ObservableProperty]
    private string _methodName = string.Empty;

    [ObservableProperty]
    private string _madhabName = string.Empty;

    [ObservableProperty]
    private string _midnightTime = string.Empty;

    /// <summary>Hides the "return to today" affordance when the screen is already showing today.</summary>
    [ObservableProperty]
    private bool _isShowingToday = true;

    public override Task OnAppearingAsync() => LoadAsync();

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    [RelayCommand]
    private Task PreviousDayAsync()
    {
        _date = _date.AddDays(-1);
        return LoadAsync();
    }

    [RelayCommand]
    private Task NextDayAsync()
    {
        _date = _date.AddDays(1);
        return LoadAsync();
    }

    [RelayCommand]
    private Task TodayAsync()
    {
        _date = _clock.Today;
        return LoadAsync();
    }

    [RelayCommand]
    private Task OpenSettingsAsync() => _navigation.GoToAsync(Routes.Settings);

    private async Task LoadAsync()
    {
        BeginLoading();

        try
        {
            Result<PrayerDayDto> day = await _prayerService.GetDayAsync(_date);

            if (day.IsFailure)
            {
                if (day.Error.Code == Error.NotConfigured.Code)
                {
                    SetFailure(OperationState.PermissionRequired, "State_LocationRequiredBody");
                    return;
                }

                SetFailure(OperationState.Error, "Common_Error");
                return;
            }

            Apply(day.Value);
            State = OperationState.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Loading the timetable failed.");
            SetFailure(OperationState.Error, "Common_Error");
        }
    }

    private void Apply(PrayerDayDto day)
    {
        bool arabic = LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar";

        HijriDateText = HijriDate.Format(day.Date, arabic);
        GregorianDateText = HijriDate.FormatGregorian(day.Date, arabic);
        LocationName = day.LocationName ?? string.Empty;
        MethodName = DisplayNames.Localized(day.Method);
        MadhabName = DisplayNames.Localized(day.Madhab);
        MidnightTime = day.Midnight.ToString("HH:mm", CultureInfo.InvariantCulture);
        IsShowingToday = day.Date == _clock.Today;

        Entries.Clear();
        foreach (PrayerEntryDto entry in day.Entries)
        {
            Entries.Add(new PrayerEntryItem(entry));
        }
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        if ((e.Kind & (SettingsChangeKind.Location | SettingsChangeKind.PrayerCalculation | SettingsChangeKind.Presentation)) == SettingsChangeKind.None)
        {
            return;
        }

        _ = _dispatcher.DispatchAsync(LoadAsync);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _settingsService.SettingsChanged -= OnSettingsChanged;
    }
}
