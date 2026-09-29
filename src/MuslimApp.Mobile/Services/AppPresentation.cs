using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Enums;
using MuslimApp.Mobile.Localization;

namespace MuslimApp.Mobile.Services;

/// <summary>
/// Applies the stored appearance and language choices to the running application.
/// </summary>
public interface IAppPresentation
{
    void Apply(AppSettings settings);

    void ApplyTheme(ThemeMode mode);

    void ApplyLanguage(Language language);
}

/// <summary>
/// The one place that touches the application-wide theme and the active culture.
/// </summary>
/// <remarks>
/// Theme switching goes through <c>UserAppTheme</c> rather than swapping resource dictionaries. That is the
/// platform's own mechanism, so <see cref="ThemeMode.System"/> keeps following the device setting for free and
/// every <c>AppThemeBinding</c> in the style sheet updates without pages being rebuilt.
/// </remarks>
internal sealed class AppPresentation : IAppPresentation
{
    public void Apply(AppSettings settings)
    {
        ApplyLanguage(settings.Language);
        ApplyTheme(settings.Theme);
    }

    public void ApplyTheme(ThemeMode mode)
    {
        AppTheme theme = mode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };

        RunOnMainThread(() =>
        {
            if (UiApplication.Current is { } application)
            {
                application.UserAppTheme = theme;
            }
        });
    }

    public void ApplyLanguage(Language language)
    {
        string culture = language == Language.English
            ? LocalizationResourceManager.EnglishCulture
            : LocalizationResourceManager.ArabicCulture;

        // Bindings update from this, so it has to happen where the UI can observe it.
        RunOnMainThread(() => LocalizationResourceManager.Current.SetCulture(culture));
    }

    private static void RunOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action();
            return;
        }

        MainThread.BeginInvokeOnMainThread(action);
    }
}
