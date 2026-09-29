using CommunityToolkit.Mvvm.Input;
using MuslimApp.Application.Common;
using MuslimApp.Mobile.Navigation;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>
/// The "more" tab: the entry point for everything that does not deserve a tab of its own.
/// </summary>
public sealed partial class MoreViewModel : BaseViewModel
{
    private readonly INavigationService _navigation;

    public MoreViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        TitleKey = "More_Title";
        State = OperationState.Success;
    }

    [RelayCommand]
    private Task OpenSettingsAsync() => _navigation.GoToAsync(Routes.Settings);

    [RelayCommand]
    private Task OpenTasbihAsync() => _navigation.GoToAsync(Routes.Tasbih);
}
