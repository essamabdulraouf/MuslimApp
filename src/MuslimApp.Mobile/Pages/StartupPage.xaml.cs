using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages;

public partial class StartupPage : AppPage
{
    public StartupPage(StartupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
