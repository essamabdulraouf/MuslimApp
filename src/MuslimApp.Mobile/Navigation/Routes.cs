namespace MuslimApp.Mobile.Navigation;

/// <summary>
/// Every navigable destination, named in one place.
/// </summary>
/// <remarks>
/// Shell routes are strings, which makes them easy to mistype and impossible to find later. Declaring them
/// here means a rename is a compile error rather than a dead navigation at runtime.
/// </remarks>
public static class Routes
{
    /// <summary>Shown while the database is prepared and the first-launch decision is made.</summary>
    public const string Startup = "//startup";

    public const string Onboarding = "//onboarding";

    public const string Home = "//main/home";
    public const string Prayer = "//main/prayer";
    public const string Azkar = "//main/azkar";
    public const string Quran = "//main/quran";
    public const string Qibla = "//main/qibla";
    public const string More = "//main/more";

    /// <summary>Pushed on top of a tab, so these are registered rather than declared in the shell.</summary>
    public const string Settings = "settings";

    public const string Location = "location";

    public const string Tasbih = "tasbih";
}
