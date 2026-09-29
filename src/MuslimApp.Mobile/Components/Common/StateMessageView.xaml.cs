using System.Windows.Input;

namespace MuslimApp.Mobile.Components.Common;

/// <summary>
/// The shared empty / error / offline / permission panel.
/// </summary>
/// <remarks>
/// One control for all four states so that "something is not available" always looks and reads the same, and so
/// each of those states can carry a way out rather than being a dead end.
/// </remarks>
public partial class StateMessageView : ContentView
{
    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(StateMessageView), string.Empty);

    public static readonly BindableProperty TitleTextProperty =
        BindableProperty.Create(nameof(TitleText), typeof(string), typeof(StateMessageView), string.Empty);

    public static readonly BindableProperty BodyTextProperty =
        BindableProperty.Create(nameof(BodyText), typeof(string), typeof(StateMessageView), string.Empty);

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(
            nameof(ActionText),
            typeof(string),
            typeof(StateMessageView),
            string.Empty,
            propertyChanged: OnActionChanged);

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(StateMessageView),
            null,
            propertyChanged: OnActionChanged);

    public static readonly BindableProperty HasActionProperty =
        BindableProperty.Create(nameof(HasAction), typeof(bool), typeof(StateMessageView), false);

    public StateMessageView()
    {
        InitializeComponent();
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public string BodyText
    {
        get => (string)GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <summary>True only when there is both a caption and a command, so a button is never shown inert.</summary>
    public bool HasAction
    {
        get => (bool)GetValue(HasActionProperty);
        private set => SetValue(HasActionProperty, value);
    }

    private static void OnActionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StateMessageView view)
        {
            view.HasAction = view.ActionCommand is not null && !string.IsNullOrEmpty(view.ActionText);
        }
    }
}
