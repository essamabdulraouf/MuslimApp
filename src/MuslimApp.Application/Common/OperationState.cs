namespace MuslimApp.Application.Common;

/// <summary>
/// The states every data-backed screen must be able to render. Defined here rather than in the UI so
/// view models across the app agree on the vocabulary.
/// </summary>
public enum OperationState
{
    Idle = 0,
    Loading = 1,
    Success = 2,
    Empty = 3,
    Error = 4,
    Offline = 5,
    PermissionRequired = 6
}
