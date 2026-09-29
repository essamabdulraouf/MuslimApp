using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Common;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Infrastructure.Persistence;
using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.Services;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>
/// The first screen: prepares the database, applies the stored appearance, then routes on.
/// </summary>
/// <remarks>
/// <para>
/// This exists because preparing the database is asynchronous and can fail, and because the first-launch
/// decision depends on a value that has to be read from it. Doing that work in <c>App</c>'s constructor would
/// mean either blocking on it or guessing, and a migration failure would surface as a crash on launch instead of
/// an explanation.
/// </para>
/// <para>
/// A failure is therefore a state on this screen, not an exception. The user is told what happened and offered a
/// retry.
/// </para>
/// </remarks>
public sealed partial class StartupViewModel : BaseViewModel
{
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly ISettingsService _settingsService;
    private readonly IAppPresentation _presentation;
    private readonly INavigationService _navigation;
    private readonly ILogger<StartupViewModel> _logger;

    public StartupViewModel(
        IDatabaseInitializer databaseInitializer,
        ISettingsService settingsService,
        IAppPresentation presentation,
        INavigationService navigation,
        ILogger<StartupViewModel> logger)
    {
        _databaseInitializer = databaseInitializer;
        _settingsService = settingsService;
        _presentation = presentation;
        _navigation = navigation;
        _logger = logger;
    }

    public override Task OnAppearingAsync() => StartAsync();

    [RelayCommand]
    private Task RetryAsync() => StartAsync();

    private async Task StartAsync()
    {
        BeginLoading();

        try
        {
            if (!await _databaseInitializer.InitializeAsync())
            {
                SetFailure(OperationState.Error, "State_DatabaseErrorBody");
                return;
            }

            AppSettings settings = await _settingsService.GetAppSettingsAsync();
            _presentation.Apply(settings);

            await _navigation.GoToAsync(settings.FirstLaunchCompleted ? Routes.Home : Routes.Onboarding);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Starting the application failed.");
            SetFailure(OperationState.Error, "State_DatabaseErrorBody");
        }
    }
}
