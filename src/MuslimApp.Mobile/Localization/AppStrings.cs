using System.Globalization;
using System.Resources;

namespace MuslimApp.Mobile.Localization;

/// <summary>
/// Reads the embedded string tables.
/// </summary>
/// <remarks>
/// The lookup goes through <see cref="ResourceManager"/> rather than a generated designer class so that the
/// key set can grow without a build-time code generation step, and so a missing key degrades to the key
/// itself instead of throwing at runtime in front of the user.
/// </remarks>
internal static class AppStrings
{
    private static readonly ResourceManager Resources = new(
        "MuslimApp.Mobile.Localization.AppStrings",
        typeof(AppStrings).Assembly);

    internal static string Get(string key, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        try
        {
            return Resources.GetString(key, culture) ?? key;
        }
        catch (MissingManifestResourceException)
        {
            // A packaging problem, not a user problem. Showing the key keeps the screen usable.
            return key;
        }
    }
}
