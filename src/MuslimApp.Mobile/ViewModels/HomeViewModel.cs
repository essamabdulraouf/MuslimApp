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
/// The home screen: the next prayer with a live countdown, and today's timetable.
/// </summary>
/// <remarks>
/// The countdown is driven by a one-second dispatcher timer that only runs while the page is on screen. It
/// recomputes from the stored target instant rather than decrementing a counter, so the display stays correct
/// after the device sleeps or the app is backgrounded.
/// </remarks>
public sealed partial class HomeViewModel : BaseViewModel, IDisposable
{
    private readonly IPrayerService _prayerService;
    private readonly ISettingsService _settingsService;
    private readonly IDateTimeProvider _clock;
    private readonly INavigationService _navigation;
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<HomeViewModel> _logger;

    private IDispatcherTimer? _timer;
    private DateTimeOffset? _nextPrayerAt;
    private DateOnly _loadedDate;
    private bool _disposed;

    public HomeViewModel(
        IPrayerService prayerService,
        ISettingsService settingsService,
        IDateTimeProvider clock,
        INavigationService navigation,
        IDispatcher dispatcher,
        ILogger<HomeViewModel> logger)
    {
        _prayerService = prayerService;
        _settingsService = settingsService;
        _clock = clock;
        _navigation = navigation;
        _dispatcher = dispatcher;
        _logger = logger;

        TitleKey = "Tab_Home";
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    public ObservableCollection<PrayerEntryItem> Entries { get; } = [];

    [ObservableProperty]
    private string _nextPrayerName = string.Empty;

    [ObservableProperty]
    private string _nextPrayerTime = string.Empty;

    [ObservableProperty]
    private string _countdown = string.Empty;

    [ObservableProperty]
    private bool _isNextPrayerTomorrow;

    [ObservableProperty]
    private string _locationName = string.Empty;

    [ObservableProperty]
    private string _hijriDateText = string.Empty;

    [ObservableProperty]
    private string _gregorianDateText = string.Empty;

    public override async Task OnAppearingAsync()
    {
        await LoadAsync();
        StartTimer();
    }

    public override Task OnDisappearingAsync()
    {
        StopTimer();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    [RelayCommand]
    private Task OpenTimetableAsync() => _navigation.GoToAsync(Routes.Prayer);

    [RelayCommand]
    private Task SetLocationAsync() => _navigation.GoToAsync(Routes.Settings);

    private async Task LoadAsync()
    {
        BeginLoading();

        try
        {
            Result<PrayerDayDto> day = await _prayerService.GetTodayAsync();

            if (day.IsFailure)
            {
                ApplyFailure(day.Error);
                return;
            }

            ApplyDay(day.Value);

            Result<NextPrayerDto> next = await _prayerService.GetNextPrayerAsync();
            if (next.IsSuccess)
            {
                ApplyNext(next.Value);
            }

            State = OperationState.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Loading the home screen failed.");
            SetFailure(OperationState.Error, "Common_Error");
        }
    }

    private void ApplyDay(PrayerDayDto day)
    {
        _loadedDate = day.Date;

        bool arabic = LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar";
        HijriDateText = HijriDate.Format(day.Date, arabic);
        GregorianDateText = HijriDate.FormatGregorian(day.Date, arabic);
        LocationName = day.LocationName ?? string.Empty;

        Entries.Clear();
        foreach (PrayerEntryDto entry in day.Entries)
        {
            Entries.Add(new PrayerEntryItem(entry));
        }
    }

    private void ApplyNext(NextPrayerDto next)
    {
        NextPrayerName = DisplayNames.Localized(next.Type);
        NextPrayerTime = next.Time.ToString("HH:mm", CultureInfo.InvariantCulture);
        IsNextPrayerTomorrow = next.IsTomorrow;
        _nextPrayerAt = next.Time;
        UpdateCountdown();
    }

    private void ApplyFailure(Error error)
    {
        if (error.Code == Error.NotConfigured.Code)
        {
            // Not an error: the user simply has not chosen a location yet, and the screen offers the fix.
            SetFailure(OperationState.PermissionRequired, "State_LocationRequiredBody");
            return;
        }

        if (error.Code == Error.LocationPermissionDenied.Code)
        {
            SetFailure(OperationState.PermissionRequired, "State_LocationDeniedBody");
            return;
        }

        SetFailure(OperationState.Error, "Common_Error");
    }

    private void StartTimer()
    {
        if (_timer is not null)
        {
            _timer.Start();
            return;
        }

        _timer = _dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void StopTimer() => _timer?.Stop();

    private void OnTick(object? sender, EventArgs e)
    {
        UpdateCountdown();

        // Crossing into the next prayer, or into a new day, changes which rows are marked as passed.
        if (_nextPrayerAt is not null && _clock.Now >= _nextPrayerAt.Value)
        {
            _ = LoadAsync();
        }
        else if (_loadedDate != DateOnly.MinValue && _clock.Today != _loadedDate && Entries.Count > 0)
        {
            _ = LoadAsync();
        }
    }

    private void UpdateCountdown()
    {
        if (_nextPrayerAt is null)
        {
            Countdown = string.Empty;
            return;
        }

        TimeSpan remaining = _nextPrayerAt.Value - _clock.Now;
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        Countdown = remaining.TotalHours >= 1d
            ? string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", (int)remaining.TotalHours, remaining.Minutes, remaining.Seconds)
            : string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", remaining.Minutes, remaining.Seconds);
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        // Presentation is included because the timetable rows carry already-translated prayer names, so a
        // language change has to rebuild them.
        const SettingsChangeKind Watched =
            SettingsChangeKind.Location | SettingsChangeKind.PrayerCalculation | SettingsChangeKind.Presentation;

        if ((e.Kind & Watched) == SettingsChangeKind.None)
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

        if (_timer is not null)
        {
            _timer.Tick -= OnTick;
            _timer.Stop();
            _timer = null;
        }
    }
}
