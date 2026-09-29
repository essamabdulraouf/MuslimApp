using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages.Settings;

public partial class SettingsPage : AppPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
