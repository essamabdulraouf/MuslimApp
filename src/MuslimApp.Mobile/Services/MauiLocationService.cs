using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Mobile.Services;

/// <summary>
/// Device positioning through MAUI Essentials.
/// </summary>
/// <remarks>
/// <para>
/// Nothing here logs coordinates. Only the outcome and the source are recorded, because a log file
/// containing a user's position would defeat the privacy promise the product is built on.
/// </para>
/// <para>
/// Accuracy is requested at <see cref="GeolocationAccuracy.Medium"/>. Prayer times shift by well under a
/// minute across a city, so a high-accuracy fix would cost battery and precision the calculation cannot use.
/// </para>
/// </remarks>
internal sealed class MauiLocationService(
    IGeolocation geolocation,
    IGeocoding geocoding,
    IConnectivityService connectivity,
    IDateTimeProvider clock,
    ILogger<MauiLocationService> logger) : ILocationService
{
    private static readonly TimeSpan FixTimeout = TimeSpan.FromSeconds(15);

    public async Task<Result<LocationResult>> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Location? location = await geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, FixTimeout),
                cancellationToken);

            if (location is null)
            {
                logger.LogInformation("No device fix was available.");
                return Result.Failure<LocationResult>(Error.LocationUnavailable);
            }

            return await BuildResultAsync(location, LocationSource.Device, cancellationToken);
        }
        catch (PermissionException)
        {
            return Result.Failure<LocationResult>(Error.LocationPermissionDenied);
        }
        catch (FeatureNotSupportedException)
        {
            return Result.Failure<LocationResult>(Error.LocationUnavailable);
        }
        catch (FeatureNotEnabledException)
        {
            // Location services are switched off at the OS level. Ordinary, not a defect.
            return Result.Failure<LocationResult>(Error.LocationUnavailable);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Requesting a device location failed.");
            return Result.Failure<LocationResult>(Error.LocationUnavailable);
        }
    }

    public async Task<Result<LocationResult>> GetLastKnownLocationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Location? location = await geolocation.GetLastKnownLocationAsync();

            return location is null
                ? Result.Failure<LocationResult>(Error.LocationUnavailable)
                : await BuildResultAsync(location, LocationSource.LastKnown, cancellationToken);
        }
        catch (PermissionException)
        {
            return Result.Failure<LocationResult>(Error.LocationPermissionDenied);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Reading the last known location failed.");
            return Result.Failure<LocationResult>(Error.LocationUnavailable);
        }
    }

    private async Task<Result<LocationResult>> BuildResultAsync(
        Location location,
        LocationSource source,
        CancellationToken cancellationToken)
    {
        if (!Coordinates.TryCreate(location.Latitude, location.Longitude, location.Altitude ?? 0d, out Coordinates coordinates))
        {
            logger.LogWarning("The platform returned coordinates outside the valid range.");
            return Result.Failure<LocationResult>(Error.LocationUnavailable);
        }

        string? placeName = await TryResolvePlaceNameAsync(location, cancellationToken);

        logger.LogInformation("A location was obtained from {Source}.", source);

        return Result.Success(new LocationResult(
            coordinates,
            source,
            clock.Now,
            location.Accuracy,
            placeName,
            TimeZoneInfo.Local.Id));
    }

    /// <summary>
    /// Turns coordinates into a city name for display only.
    /// </summary>
    /// <remarks>
    /// Reverse geocoding is a network call on both platforms, so it is attempted only when a connection
    /// exists and is allowed to fail silently: a timetable with no city label is still a correct timetable.
    /// </remarks>
    private async Task<string?> TryResolvePlaceNameAsync(Location location, CancellationToken cancellationToken)
    {
        if (!connectivity.IsConnected)
        {
            return null;
        }

        try
        {
            IEnumerable<Placemark> placemarks = await geocoding.GetPlacemarksAsync(location)
                .WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

            Placemark? placemark = placemarks.FirstOrDefault();

            return placemark?.Locality
                ?? placemark?.SubAdminArea
                ?? placemark?.AdminArea
                ?? placemark?.CountryName;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogDebug("Reverse geocoding was unavailable; continuing without a city name.");
            return null;
        }
    }
}
