using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages.Settings;

public partial class LocationPage : AppPage
{
    public LocationPage(LocationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
