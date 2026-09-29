using System.Globalization;

namespace MuslimApp.Mobile.Localization;

/// <summary>
/// XAML markup extension: <c>Text="{loc:Translate Home_Greeting}"</c>.
/// </summary>
/// <remarks>
/// It returns a binding rather than a string so the text follows a language change without the page being
/// rebuilt.
/// </remarks>
[ContentProperty(nameof(Key))]
public sealed class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider) =>
        new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationResourceManager.Current,
        };

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        ProvideValue(serviceProvider);
}

/// <summary>
/// Binds a control's reading direction to the current language.
/// </summary>
public sealed class FlowDirectionExtension : IMarkupExtension<BindingBase>
{
    public BindingBase ProvideValue(IServiceProvider serviceProvider) =>
        new Binding
        {
            Mode = BindingMode.OneWay,
            Path = nameof(LocalizationResourceManager.FlowDirection),
            Source = LocalizationResourceManager.Current,
        };

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        ProvideValue(serviceProvider);
}

/// <summary>
/// Formats a <see cref="TimeSpan"/> countdown for display.
/// </summary>
/// <remarks>
/// Digits are written with the invariant culture on purpose. Arabic-Indic digits are correct in prose but a
/// prayer timetable is read at a glance, and the mockups use Western digits throughout.
/// </remarks>
public sealed class CountdownConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan remaining)
        {
            return string.Empty;
        }

        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        return remaining.TotalHours >= 1d
            ? string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", (int)remaining.TotalHours, remaining.Minutes, remaining.Seconds)
            : string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", remaining.Minutes, remaining.Seconds);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("A countdown is display only.");
}

/// <summary>
/// Formats a clock time for the timetable.
/// </summary>
public sealed class ClockTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        DateTimeOffset? instant = value switch
        {
            DateTimeOffset offset => offset,
            DateTime dateTime => new DateTimeOffset(dateTime),
            _ => null,
        };

        return instant is null
            ? string.Empty
            : instant.Value.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("A prayer time is display only.");
}
