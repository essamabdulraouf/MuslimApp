using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Features.Prayer;
using MuslimApp.Application.Features.Settings;

namespace MuslimApp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the application layer. It contributes no platform or persistence implementations, so
    /// this can be called from the mobile app, the optional API, or a test host unchanged.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Both services are stateless and both are consumed by long-lived view models. Persistence is
        // reached through a DbContext factory, so there is no scoped dependency to capture.
        services.TryAddSingleton<IPrayerService, PrayerService>();
        services.TryAddSingleton<ISettingsService, SettingsService>();

        return services;
    }
}
