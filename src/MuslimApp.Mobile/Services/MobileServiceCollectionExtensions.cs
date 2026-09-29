using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.Pages;
using MuslimApp.Mobile.Pages.Azkar;
using MuslimApp.Mobile.Pages.Home;
using MuslimApp.Mobile.Pages.More;
using MuslimApp.Mobile.Pages.Onboarding;
using MuslimApp.Mobile.Pages.Prayer;
using MuslimApp.Mobile.Pages.Qibla;
using MuslimApp.Mobile.Pages.Quran;
using MuslimApp.Mobile.Pages.Settings;
using MuslimApp.Mobile.Pages.Tasbih;
using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Services;

public static class MobileServiceCollectionExtensions
{
    /// <summary>
    /// Registers the device adapters, navigation, pages and view models.
    /// </summary>
    /// <remarks>
    /// The device adapters live in this project rather than in Infrastructure on purpose: they depend on MAUI
    /// Essentials, and Infrastructure is deliberately kept free of any UI framework so it stays usable from a
    /// test host or the optional API.
    /// </remarks>
    public static IServiceCollection AddMobileServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // MAUI Essentials singletons, taken as interfaces so the adapters above them remain substitutable.
        services.TryAddSingleton(Geolocation.Default);
        services.TryAddSingleton(Geocoding.Default);
        services.TryAddSingleton(Connectivity.Current);

        services.TryAddSingleton<IConnectivityService, MauiConnectivityService>();
        services.TryAddSingleton<ILocationService, MauiLocationService>();
        services.TryAddSingleton<IPermissionService, MauiPermissionService>();
        services.TryAddSingleton<INotificationService, DeferredNotificationService>();

        services.TryAddSingleton<IAppPresentation, AppPresentation>();
        services.TryAddSingleton<INavigationService, ShellNavigationService>();

        AddViewModels(services);
        AddPages(services);

        return services;
    }

    /// <remarks>
    /// Tab view models are singletons because their tab is never destroyed and rebuilding their state on every
    /// visit would be wasted work. Pushed pages are transient so that returning to one starts clean.
    /// </remarks>
    private static void AddViewModels(IServiceCollection services)
    {
        services.TryAddSingleton<StartupViewModel>();
        services.TryAddSingleton<OnboardingViewModel>();
        services.TryAddSingleton<HomeViewModel>();
        services.TryAddSingleton<PrayerViewModel>();
        services.TryAddSingleton<MoreViewModel>();

        services.TryAddTransient<SettingsViewModel>();
        services.TryAddTransient<LocationViewModel>();
    }

    private static void AddPages(IServiceCollection services)
    {
        services.TryAddSingleton<StartupPage>();
        services.TryAddSingleton<OnboardingPage>();
        services.TryAddSingleton<HomePage>();
        services.TryAddSingleton<PrayerPage>();
        services.TryAddSingleton<AzkarPage>();
        services.TryAddSingleton<QuranPage>();
        services.TryAddSingleton<QiblaPage>();
        services.TryAddSingleton<MorePage>();

        services.TryAddTransient<SettingsPage>();
        services.TryAddTransient<LocationPage>();
        services.TryAddTransient<TasbihPage>();
    }
}
