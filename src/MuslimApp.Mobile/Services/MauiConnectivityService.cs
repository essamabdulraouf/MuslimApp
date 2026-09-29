using MuslimApp.Application.Abstractions.Platform;

namespace MuslimApp.Mobile.Services;

/// <summary>
/// Network reachability.
/// </summary>
/// <remarks>
/// Only optional online work may consult this. Prayer times, azkar, the Quran and the qibla are all computed
/// or read locally and must never branch on connectivity.
/// </remarks>
internal sealed class MauiConnectivityService(IConnectivity connectivity) : IConnectivityService
{
    public bool IsConnected => connectivity.NetworkAccess == NetworkAccess.Internet;
}
