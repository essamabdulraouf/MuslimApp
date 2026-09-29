namespace MuslimApp.Mobile;

public partial class App : UiApplication
{
    /// <remarks>
    /// <para>
    /// The shell is created here rather than injected. Constructor parameters are resolved before the
    /// application object exists, and the shell's XAML reads design tokens out of the application's resource
    /// dictionaries — so an injected shell would be parsed before those dictionaries were loaded and every
    /// <c>StaticResource</c> in it would fail. Creating it after <c>InitializeComponent</c> guarantees the order.
    /// The shell's pages are still resolved from the container, because Shell resolves page templates through the
    /// service provider at navigation time.
    /// </para>
    /// <para>
    /// Nothing is loaded at this point: the database is prepared by the startup page, which is the shell's first
    /// route, so a failure there can be shown to the user instead of crashing the launch.
    /// </para>
    /// </remarks>
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
