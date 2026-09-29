using MuslimApp.Mobile.Navigation;
using MuslimApp.Mobile.Pages.Settings;
using MuslimApp.Mobile.Pages.Tasbih;

namespace MuslimApp.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterDetailRoutes();
    }

    /// <summary>
    /// Pages that are pushed on top of a tab rather than being a tab themselves.
    /// </summary>
    /// <remarks>
    /// Registering them means they are resolved from the dependency injection container when navigated to, which
    /// is how their view models arrive constructed.
    /// </remarks>
    private static void RegisterDetailRoutes()
    {
        Routing.RegisterRoute(Routes.Settings, typeof(SettingsPage));
        Routing.RegisterRoute(Routes.Location, typeof(LocationPage));
        Routing.RegisterRoute(Routes.Tasbih, typeof(TasbihPage));
    }
}
