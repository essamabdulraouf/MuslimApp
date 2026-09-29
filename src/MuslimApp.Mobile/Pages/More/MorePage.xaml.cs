using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages.More;

public partial class MorePage : AppPage
{
    public MorePage(MoreViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
