using System.Globalization;

namespace MuslimApp.Mobile.Localization;

/// <summary>
/// Resolves a resource key held in a view model to its translated text.
/// </summary>
/// <remarks>
/// View models carry keys rather than sentences, so nothing outside this layer decides wording or language. This
/// converter is what lets a bound <c>MessageKey</c> render as text.
/// </remarks>
public sealed class TranslateKeyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string key && key.Length > 0
            ? LocalizationResourceManager.Current[key]
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("Translated text is display only.");
}

/// <summary>True when a string has content; used to hide optional labels and chips.</summary>
public sealed class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && !string.IsNullOrWhiteSpace(text);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("One-way only.");
}

/// <summary>Inverts a boolean, for the common "hide while busy" case.</summary>
public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not bool flag || !flag;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not bool flag || !flag;
}

/// <summary>
/// Picks the filled or hollow radio glyph for a selection row.
/// </summary>
/// <remarks>
/// Radio state is drawn with an icon glyph rather than a <c>RadioButton</c> because the platform controls cannot
/// be restyled consistently across Android and iOS, and a settings list that looks different on each platform
/// undermines a single design system.
/// </remarks>
public sealed class SelectionGlyphConverter : IValueConverter
{
    private const string Selected = "\ue837";
    private const string Unselected = "\ue836";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Selected : Unselected;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("One-way only.");
}
