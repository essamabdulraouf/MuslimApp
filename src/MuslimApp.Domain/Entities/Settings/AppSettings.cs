using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Settings;

/// <summary>
/// Single-row application state. The permission flags record that the app has *asked*, not that it was
/// granted, so onboarding never nags a user who deliberately declined.
/// </summary>
public class AppSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public Language Language { get; set; } = Language.Arabic;

    public ThemeMode Theme { get; set; } = ThemeMode.System;

    public bool FirstLaunchCompleted { get; set; }

    public bool LocationPermissionRequested { get; set; }

    public bool NotificationPermissionRequested { get; set; }

    public int ContentSchemaVersion { get; set; }

    public string? ContentVersion { get; set; }
}
