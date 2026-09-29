namespace MuslimApp.Domain.Common;

/// <summary>
/// The geographical result of a qibla calculation. This is deliberately separate from any device
/// compass heading: <see cref="BearingFromTrueNorth"/> depends only on coordinates, so it can be
/// tested and cached, while magnetic heading is a live sensor concern.
/// </summary>
public sealed record QiblaResult(
    Coordinates Coordinates,
    double BearingFromTrueNorth,
    double DistanceToKaabaKm);
