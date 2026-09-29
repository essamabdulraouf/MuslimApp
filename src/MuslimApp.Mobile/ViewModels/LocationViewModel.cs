using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Common;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Mobile.Localization;
using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.ViewModels.Items;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>
/// Changing the location after first launch: a device fix, or a city from the built-in list.
/// </summary>
/// <remarks>
/// The same two paths as onboarding are offered here, because a user who declined the permission originally
/// must be able to change their mind, and a traveller must be able to pin a city without the device deciding.
/// </remarks>
public sealed partial class LocationViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ILocationService _locationService;
    private readonly IPermissionService _permissionService;
    private readonly IDateTimeProvider _clock;
    private readonly INavigationService _navigation;
    private readonly ILogger<LocationViewModel> _logger;

    public LocationViewModel(
        ISettingsService settingsService,
        ILocationService locationService,
        IPermissionService permissionService,
        IDateTimeProvider clock,
        INavigationService navigation,
        ILogger<LocationViewModel> logger)
    {
        _settingsService = settingsService;
        _locationService = locationService;
        _permissionService = permissionService;
        _clock = clock;
        _navigation = navigation;
        _logger = logger;

        TitleKey = "Settings_Location";
        State = OperationState.Success;
    }

    public ObservableCollection<CityItem> Cities { get; } = [];

    [ObservableProperty]
    private string _currentLocationName = string.Empty;

    [ObservableProperty]
    private string _citySearch = string.Empty;

    [ObservableProperty]
    private bool _isWorking;

    [ObservableProperty]
    private string _noticeKey = string.Empty;

    public bool HasNotice => !string.IsNullOrEmpty(NoticeKey);

    public override async Task OnAppearingAsync()
    {
        try
        {
            PrayerSettings settings = await _settingsService.GetPrayerSettingsAsync();
            CurrentLocationName = settings.HasLocation && !string.IsNullOrWhiteSpace(settings.LocationName)
                ? settings.LocationName!
                : LocalizationResourceManager.Current["Settings_LocationUnset"];

            if (Cities.Count == 0)
            {
                Populate(CityCatalog.Cities);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Loading the location screen failed.");
            NoticeKey = "Common_Error";
        }
    }

    [RelayCommand]
    private async Task UseMyLocationAsync()
    {
        if (IsWorking)
        {
            return;
        }

        IsWorking = true;
        NoticeKey = string.Empty;

        try
        {
            if (!await _permissionService.RequestLocationAsync())
            {
                NoticeKey = "State_LocationDeniedBody";
                return;
            }

            Result<LocationResult> location = await _locationService.GetCurrentLocationAsync();

            if (location.IsFailure)
            {
                NoticeKey = "State_LocationRequiredBody";
                return;
            }

            await _settingsService.SetLocationAsync(location.Value);
            await _navigation.GoBackAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Updating the location from the device failed.");
            NoticeKey = "Common_Error";
        }
        finally
        {
            IsWorking = false;
        }
    }

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
            await _settingsService.SetLocationAsync(city.Option.ToLocationResult(_clock.Now, arabic));
            await _navigation.GoBackAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Saving a manually chosen city failed.");
            NoticeKey = "Common_Error";
        }
        finally
        {
            IsWorking = false;
        }
    }

    private void Populate(IEnumerable<CityOption> cities)
    {
        bool arabic = LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar";

        Cities.Clear();
        foreach (CityOption city in cities)
        {
            Cities.Add(new CityItem(city, arabic));
        }
    }

    partial void OnNoticeKeyChanged(string value) => OnPropertyChanged(nameof(HasNotice));

    partial void OnCitySearchChanged(string value) =>
        Populate(CityCatalog.Search(value, LocalizationResourceManager.Current.Culture.TwoLetterISOLanguageName == "ar"));
}
