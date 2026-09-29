using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages.Home;

public partial class HomePage : AppPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
