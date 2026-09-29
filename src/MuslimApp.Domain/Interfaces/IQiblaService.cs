using MuslimApp.Domain.Common;

namespace MuslimApp.Domain.Interfaces;

/// <summary>
/// Great-circle bearing from a position to the Kaaba. Deliberately knows nothing about the device
/// compass, magnetic declination, or calibration.
/// </summary>
public interface IQiblaService
{
    Task<QiblaResult> CalculateAsync(Coordinates coordinates, CancellationToken cancellationToken = default);
}
