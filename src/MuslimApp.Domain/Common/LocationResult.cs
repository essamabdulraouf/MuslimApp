using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Common;

/// <summary>
/// A position together with its provenance. <see cref="Source"/> lets the UI tell the user whether
/// the times come from a live fix, a stored city, or a stale cache.
/// </summary>
public sealed record LocationResult(
    Coordinates Coordinates,
    LocationSource Source,
    DateTimeOffset RetrievedAt,
    double? AccuracyMetres = null,
    string? LocationName = null,
    string? TimeZoneId = null);
