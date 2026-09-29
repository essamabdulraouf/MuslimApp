using CommunityToolkit.Mvvm.ComponentModel;
using MuslimApp.Application.Common;
using MuslimApp.Mobile.Localization;

namespace MuslimApp.Mobile.ViewModels;

/// <summary>
/// Shared view model behaviour: the screen state machine, and the lifecycle hooks the pages call.
/// </summary>
/// <remarks>
/// Every data-backed screen reports exactly one <see cref="OperationState"/>, and the page binds its sections
/// to the matching <c>Is…</c> flag. That is what keeps loading, empty, error, offline and permission states from
/// being reinvented — differently — on each page.
/// </remarks>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private OperationState _state = OperationState.Idle;

    [ObservableProperty]
    private string _titleKey = string.Empty;

    /// <summary>A resource key, not a message. The view resolves it, so the text follows the language.</summary>
    [ObservableProperty]
    private string _messageKey = string.Empty;

    /// <summary>Exposed so XAML can bind translated text without every page reaching for the singleton.</summary>
    public LocalizationResourceManager Localization => LocalizationResourceManager.Current;

    public bool IsLoading => State == OperationState.Loading;

    public bool IsSuccess => State == OperationState.Success;

    public bool IsEmpty => State == OperationState.Empty;

    public bool IsError => State == OperationState.Error;

    public bool IsOffline => State == OperationState.Offline;

    public bool IsPermissionRequired => State == OperationState.PermissionRequired;

    /// <summary>True for every state that replaces the content with an explanation.</summary>
    public bool IsInterrupted => IsEmpty || IsError || IsOffline || IsPermissionRequired;

    public virtual Task OnAppearingAsync() => Task.CompletedTask;

    public virtual Task OnDisappearingAsync() => Task.CompletedTask;

    protected void SetFailure(OperationState state, string messageKey)
    {
        MessageKey = messageKey;
        State = state;
    }

    protected void BeginLoading()
    {
        MessageKey = string.Empty;
        State = OperationState.Loading;
    }

    partial void OnStateChanged(OperationState value)
    {
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(IsSuccess));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsError));
        OnPropertyChanged(nameof(IsOffline));
        OnPropertyChanged(nameof(IsPermissionRequired));
        OnPropertyChanged(nameof(IsInterrupted));
    }
}
