using System.Diagnostics;
using MuslimApp.Mobile.Localization;
using MuslimApp.Mobile.ViewModels;

namespace MuslimApp.Mobile.Pages;

/// <summary>
/// The base every page in the app derives from.
/// </summary>
/// <remarks>
/// <para>
/// It does two things. It binds the page's reading direction to the active language, which is why no page sets
/// <c>FlowDirection</c> itself and why Arabic renders right to left everywhere without per-page work. And it
/// forwards the page lifecycle to the view model, so view models never have to be wired to page events by hand.
/// </para>
/// <para>
/// The forwarding is deliberately defensive: an exception escaping an <c>async void</c> lifecycle handler would
/// terminate the process, so anything a view model fails to handle is contained here rather than crashing the
/// app on navigation.
/// </para>
/// </remarks>
public abstract class AppPage : ContentPage
{
    protected AppPage()
    {
        SetBinding(
            FlowDirectionProperty,
            new Binding(nameof(LocalizationResourceManager.FlowDirection), source: LocalizationResourceManager.Current));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BaseViewModel viewModel)
        {
            Run(viewModel.OnAppearingAsync);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is BaseViewModel viewModel)
        {
            Run(viewModel.OnDisappearingAsync);
        }
    }

    private static void Run(Func<Task> work) => _ = RunAsync(work);

    /// <remarks>
    /// Not offloaded to a worker thread. Lifecycle events arrive on the UI thread, so awaiting here keeps the
    /// continuations there too — which matters because the view models update observable collections that the
    /// platform collection views are bound to.
    /// </remarks>
    private static async Task RunAsync(Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"A page lifecycle handler failed: {exception}");
        }
    }
}
