using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Common;
using MuslimApp.Application.DTOs;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;
using MuslimApp.Mobile.Localization;
using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.ViewModels.Items;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>The steps of the first-launch flow, in order.</summary>
public enum OnboardingStep
{
    Welcome = 0,
    Location = 1,
    CityPicker = 2,
    Method = 3,
    Notifications = 4,
    Ready = 5,
}

/// <summary>
/// First launch: welcome, location, calculation convention, notifications, done.
/// </summary>
/// <remarks>
/// <para>
/// The steps live in one view model and one page rather than a stack of pages. The flow branches (a granted
/// location skips the city picker; a denied one lands on it) and the earlier answers influence the later
/// defaults, so a single state machine is easier to reason about than six pages passing parameters.
/// </para>
/// <para>
/// Every permission step can be declined and the flow still completes. Declining location leaves the timetable
/// unconfigured and the home screen explains how to fix it; declining notifications simply means no reminders.
/// Neither is a dead end, because a user who is uncomfortable granting a permission must still get the app.
/// </para>
/// </remarks>
public sealed partial class OnboardingViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ILocationService _locationService;
    private readonly IPermissionService _permissionService;
    private readonly IDateTimeProvider _clock;
    private readonly INavigationService _navigation;
    private readonly ILogger<OnboardingViewModel> _logger;

    public OnboardingViewModel(
        ISettingsService settingsService,
        ILocationService locationService,
        IPermissionService permissionService,
        IDateTimeProvider clock,
        INavigationService navigation,
        ILogger<OnboardingViewModel> logger)
    {
        _settingsService = settingsService;
        _locationService = locationService;
        _permissionService = permissionService;
        _clock = clock;
        _navigation = navigation;
        _logger = logger;

        State = OperationState.Success;
    }

    public ObservableCollection<MethodOptionItem> Methods { get; } = [];

    public ObservableCollection<CityItem> Cities { get; } = [];

    [ObservableProperty]
    private OnboardingStep _step = OnboardingStep.Welcome;

    [ObservableProperty]
    private bool _isWorking;

    /// <summary>Set when a location attempt failed, so the step can explain it without leaving the flow.</summary>
    [ObservableProperty]
    private string _locationMessageKey = string.Empty;

    [ObservableProperty]
    private string _selectedLocationName = string.Empty;

    [ObservableProperty]
    private string _citySearch = string.Empty;

    public bool IsWelcome => Step == OnboardingStep.Welcome;

    public bool IsLocation => Step == OnboardingStep.Location;

    public bool IsCityPicker => Step == OnboardingStep.CityPicker;

    public bool IsMethod => Step == OnboardingStep.Method;

    public bool IsNotifications => Step == OnboardingStep.Notifications;

    public bool IsReady => Step == OnboardingStep.Ready;

    public bool CanGoBack => Step != OnboardingStep.Welcome && Step != OnboardingStep.Ready;

    public bool HasLocationMessage => !string.IsNullOrEmpty(LocationMessageKey);

    public bool HasSelectedLocation => !string.IsNullOrEmpty(SelectedLocationName);

    public override async Task OnAppearingAsync()
    {
        try
        {
            if (Methods.Count == 0)
            {
                await LoadMethodsAsync();
            }

            if (Cities.Count == 0)
            {
                PopulateCities(CityCatalog.Cities);
            }
        }
        catch (Exception exception)
        {
            // The welcome step has nothing to load, so the flow can still start even if this failed.
            _logger.LogError(exception, "Preparing the onboarding options failed.");
        }
    }

    [RelayCommand]
    private void Start() => Step = OnboardingStep.Location;

    [RelayCommand]
    private void Back() =>
        Step = Step switch
        {
            OnboardingStep.CityPicker => OnboardingStep.Location,
            OnboardingStep.Method => OnboardingStep.Location,
            OnboardingStep.Notifications => OnboardingStep.Method,
            _ => OnboardingStep.Welcome,
        };

    [RelayCommand]
    private async Task UseMyLocationAsync()
    {
        if (IsWorking)
        {
            return;
        }

        IsWorking = true;
        LocationMessageKey = string.Empty;

        try
        {
            // The flag records that we asked, so a later launch does not prompt a user who declined.
            await _settingsService.MarkLocationPermissionRequestedAsync();

            if (!await _permissionService.RequestLocationAsync())
            {
                LocationMessageKey = "State_LocationDeniedBody";
                Step = OnboardingStep.CityPicker;
                return;
            }

            Result<LocationResult> location = await _locationService.GetCurrentLocationAsync();

            if (location.IsFailure)
            {
                // A granted permission with no fix is common indoors; the city list is the way forward.
                LocationMessageKey = "State_LocationRequiredBody";
                Step = OnboardingStep.CityPicker;
                return;
            }

            await ApplyLocationAsync(location.Value, suggestedMethod: null);
            Step = OnboardingStep.Method;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "The location step failed.");
            LocationMessageKey = "Common_Error";
            Step = OnboardingStep.CityPicker;
        }
        finally
        {
            IsWorking = false;
        }
    }

    [RelayCommand]
    private void ChooseCity()
    {
        LocationMessageKey = string.Empty;
        Step = OnboardingStep.CityPicker;
    }

    [RelayCommand]
    private void SkipLocation() => Step = OnboardingStep.Method;

    [RelayCommand]
    private async Task SelectCityAsync(CityItem? city)
    {
        if (city is null || IsWorking)
        {
            return;
        }

        IsWorking = true;

        try
        {
            bool arabic = LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar";
            LocationResult location = city.Option.ToLocationResult(_clock.Now, arabic);

            await ApplyLocationAsync(location, city.Option.SuggestedMethod);
            Step = OnboardingStep.Method;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Saving a manually chosen city failed.");
            LocationMessageKey = "Common_Error";
        }
        finally
        {
            IsWorking = false;
        }
    }

    [RelayCommand]
    private async Task SelectMethodAsync(MethodOptionItem? item)
    {
        if (item is null)
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
    private void ContinueFromMethod() => Step = OnboardingStep.Notifications;

    [RelayCommand]
    private async Task EnableNotificationsAsync()
    {
        if (IsWorking)
        {
            return;
        }

        IsWorking = true;

        try
        {
            await _settingsService.MarkNotificationPermissionRequestedAsync();
            await _permissionService.RequestNotificationAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Requesting notification permission failed.");
        }
        finally
        {
            IsWorking = false;
            // A denial is not a blocker: the flow continues either way.
            Step = OnboardingStep.Ready;
        }
    }

    [RelayCommand]
    private async Task SkipNotificationsAsync()
    {
        await _settingsService.MarkNotificationPermissionRequestedAsync();
        Step = OnboardingStep.Ready;
    }

    [RelayCommand]
    private async Task FinishAsync()
    {
        await _settingsService.CompleteOnboardingAsync();
        await _navigation.GoToAsync(Routes.Home);
    }

    private async Task LoadMethodsAsync()
    {
        CalculationMethod current = (await _settingsService.GetPrayerSettingsAsync()).CalculationMethod;

        Methods.Clear();
        foreach (CalculationMethodOptionDto option in CalculationMethodOptions.All)
        {
            // Custom is configured from settings once the user knows what they are overriding; offering it
            // during first launch would ask for angles before anything has been explained.
            if (option.Method == CalculationMethod.Custom)
            {
                continue;
            }

            Methods.Add(new MethodOptionItem(option, option.Method == current));
        }
    }

    private void PopulateCities(IEnumerable<CityOption> cities)
    {
        bool arabic = LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar";

        Cities.Clear();
        foreach (CityOption city in cities)
        {
            Cities.Add(new CityItem(city, arabic));
        }
    }

    private async Task ApplyLocationAsync(LocationResult location, CalculationMethod? suggestedMethod)
    {
        await _settingsService.SetLocationAsync(location);
        SelectedLocationName = location.LocationName ?? string.Empty;

        if (suggestedMethod is not null)
        {
            await _settingsService.SetCalculationMethodAsync(suggestedMethod.Value);
        }

        await LoadMethodsAsync();
    }

    partial void OnStepChanged(OnboardingStep value)
    {
        OnPropertyChanged(nameof(IsWelcome));
        OnPropertyChanged(nameof(IsLocation));
        OnPropertyChanged(nameof(IsCityPicker));
        OnPropertyChanged(nameof(IsMethod));
        OnPropertyChanged(nameof(IsNotifications));
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(CanGoBack));
    }

    partial void OnLocationMessageKeyChanged(string value) => OnPropertyChanged(nameof(HasLocationMessage));

    partial void OnSelectedLocationNameChanged(string value) => OnPropertyChanged(nameof(HasSelectedLocation));

    partial void OnCitySearchChanged(string value) =>
        PopulateCities(CityCatalog.Search(value, LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar"));
}
