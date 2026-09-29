using Microsoft.Extensions.Logging;
using MuslimApp.Application.Abstractions.Platform;

namespace MuslimApp.Mobile.Services;

/// <summary>
/// Runtime permissions through MAUI Essentials.
/// </summary>
/// <remarks>
/// A denial is returned as <c>false</c>, never thrown. Declining location or notifications is a legitimate
/// choice, and the application has a complete path for both: a manually chosen city, and a timetable without
/// reminders.
/// </remarks>
internal sealed class MauiPermissionService(ILogger<MauiPermissionService> logger) : IPermissionService
{
    public Task<bool> IsLocationGrantedAsync() =>
        CheckAsync<Permissions.LocationWhenInUse>();

    public Task<bool> RequestLocationAsync() =>
        RequestAsync<Permissions.LocationWhenInUse>("location");

    public Task<bool> IsNotificationGrantedAsync() =>
        CheckAsync<Permissions.PostNotifications>();

    public Task<bool> RequestNotificationAsync() =>
        RequestAsync<Permissions.PostNotifications>("notifications");

    private async Task<bool> CheckAsync<TPermission>()
        where TPermission : Permissions.BasePermission, new()
    {
        try
        {
            PermissionStatus status = await MainThread.InvokeOnMainThreadAsync(Permissions.CheckStatusAsync<TPermission>);
            return status == PermissionStatus.Granted;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Checking a permission status failed.");
            return false;
        }
    }

    private async Task<bool> RequestAsync<TPermission>(string permissionName)
        where TPermission : Permissions.BasePermission, new()
    {
        try
        {
            PermissionStatus status = await MainThread.InvokeOnMainThreadAsync(Permissions.CheckStatusAsync<TPermission>);

            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            // The platform prompt must be raised from the UI thread or it is silently dropped on Android.
            status = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<TPermission>);

            logger.LogInformation("The {Permission} permission ended as {Status}.", permissionName, status);
            return status == PermissionStatus.Granted;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Requesting the {Permission} permission failed.", permissionName);
            return false;
        }
    }
}
