using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages.Onboarding;

public partial class OnboardingPage : AppPage
{
    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
