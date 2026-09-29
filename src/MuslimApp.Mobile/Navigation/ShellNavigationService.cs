using Microsoft.Extensions.Logging;

namespace MuslimApp.Mobile.Navigation;

/// <summary>
/// Shell-backed navigation.
/// </summary>
/// <remarks>
/// Failures are logged and swallowed. A navigation that cannot complete — usually because the shell is being
/// torn down — is not worth crashing the application over, and the user stays on a working screen.
/// </remarks>
internal sealed class ShellNavigationService(ILogger<ShellNavigationService> logger) : INavigationService
{
    public Task GoToAsync(string route) => NavigateAsync(route, null);

    public Task GoToAsync(string route, IDictionary<string, object> parameters) => NavigateAsync(route, parameters);

    public async Task GoBackAsync()
    {
        try
        {
            Shell? shell = Shell.Current;
            if (shell is null)
            {
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() => shell.GoToAsync(".."));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Navigating back failed.");
        }
    }

    public async Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        Page? page = UiApplication.Current?.MainPage;
        if (page is null)
        {
            return false;
        }

        return await MainThread.InvokeOnMainThreadAsync(() => page.DisplayAlert(title, message, accept, cancel));
    }

    public async Task<string?> ChooseAsync(string title, string cancel, params string[] options)
    {
        Page? page = UiApplication.Current?.MainPage;
        if (page is null)
        {
            return null;
        }

        string? choice = await MainThread.InvokeOnMainThreadAsync(
            () => page.DisplayActionSheet(title, cancel, null, options));

        // The platform returns the cancel caption rather than null when the sheet is dismissed.
        return string.IsNullOrEmpty(choice) || string.Equals(choice, cancel, StringComparison.Ordinal)
            ? null
            : choice;
    }

    private async Task NavigateAsync(string route, IDictionary<string, object>? parameters)
    {
        try
        {
            Shell? shell = Shell.Current;
            if (shell is null)
            {
                logger.LogWarning("Navigation to {Route} was requested before the shell existed.", route);
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() => parameters is null
                ? shell.GoToAsync(route)
                : shell.GoToAsync(route, parameters));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Navigation to {Route} failed.", route);
        }
    }
}
