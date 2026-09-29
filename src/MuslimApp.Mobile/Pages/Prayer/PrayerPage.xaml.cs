using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages.Prayer;

public partial class PrayerPage : AppPage
{
    public PrayerPage(PrayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
