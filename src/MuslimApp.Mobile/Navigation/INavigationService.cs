namespace MuslimApp.Mobile.Navigation;

/// <summary>
/// Navigation as the view models see it.
/// </summary>
/// <remarks>
/// View models never touch <see cref="Shell"/> directly. That keeps them testable and keeps every route
/// string behind <see cref="Routes"/>.
/// </remarks>
public interface INavigationService
{
    Task GoToAsync(string route);

    Task GoToAsync(string route, IDictionary<string, object> parameters);

    Task GoBackAsync();

    /// <summary>A modal question with two answers. Returns true when the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);

    /// <summary>A single-choice list. Returns null when the user dismisses it.</summary>
    Task<string?> ChooseAsync(string title, string cancel, params string[] options);
}
