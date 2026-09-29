using System.ComponentModel;
using System.Globalization;

namespace MuslimApp.Mobile.Localization;

/// <summary>
/// The single source of localized text and reading direction for the running application.
/// </summary>
/// <remarks>
/// Bindings target the indexer, so raising a change notification for the whole indexer is enough to retranslate
/// every bound label when the language changes. That avoids restarting the application to switch language.
/// </remarks>
public sealed class LocalizationResourceManager : INotifyPropertyChanged
{
    public const string ArabicCulture = "ar";
    public const string EnglishCulture = "en";

    private static readonly string[] RightToLeftLanguages = ["ar", "fa", "ur", "he"];

    private CultureInfo _culture = CultureInfo.GetCultureInfo(ArabicCulture);

    private LocalizationResourceManager()
    {
    }

    public static LocalizationResourceManager Current { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Indexed by resource key; this is what XAML binds to.</summary>
    public string this[string key] => AppStrings.Get(key, _culture);

    public CultureInfo Culture => _culture;

    /// <summary>
    /// The reading direction implied by the current language. Arabic is the product's primary language, so
    /// right to left is the default rather than an adaptation.
    /// </summary>
    public FlowDirection FlowDirection =>
        RightToLeftLanguages.Contains(_culture.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

    public void SetCulture(string cultureName)
    {
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            // An unknown stored language must not lock the user out, so fall back to the primary language.
            culture = CultureInfo.GetCultureInfo(ArabicCulture);
        }

        if (string.Equals(culture.Name, _culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _culture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        // An empty name invalidates every binding on this object, which is exactly what a language switch needs.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FlowDirection)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
    }

    internal string Translate(string key) => this[key];
}
