using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Settings;

/// <summary>
/// Theme choice as the presentation layer consumes it. Projected from <see cref="AppSettings.Theme"/>
/// rather than stored separately, so there is no second copy of the user's choice to drift.
/// </summary>
public sealed record ThemeSettings(ThemeMode Mode)
{
    public bool FollowsSystem => Mode == ThemeMode.System;

    public static ThemeSettings Default { get; } = new(ThemeMode.System);
}

/// <summary>
/// Language choice plus the layout direction it implies. Projected from
/// <see cref="AppSettings.Language"/>.
/// </summary>
public sealed record LanguageSettings(Language Language)
{
    public string CultureCode => Language == Language.Arabic ? "ar" : "en";

    public bool IsRightToLeft => Language == Language.Arabic;

    public static LanguageSettings Default { get; } = new(Enums.Language.Arabic);
}
