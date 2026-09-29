namespace MuslimApp.Domain.Common;

/// <summary>
/// A machine-readable failure. <see cref="Code"/> is stable and safe to switch on; the UI resolves
/// it to a localized message rather than showing <see cref="Message"/> to the user.
/// </summary>
public readonly record struct Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error LocationUnavailable { get; } = new("location.unavailable", "A device location could not be obtained.");

    public static Error LocationPermissionDenied { get; } = new("location.permission_denied", "Location permission was denied.");

    public static Error CompassUnavailable { get; } = new("compass.unavailable", "This device has no usable compass.");

    public static Error NotificationPermissionDenied { get; } = new("notifications.permission_denied", "Notification permission was denied.");

    public static Error NotConfigured { get; } = new("settings.not_configured", "The app has not been configured yet.");

    public static Error Unexpected { get; } = new("unexpected", "An unexpected error occurred.");
}

/// <summary>
/// Outcome of an operation that is expected to fail for ordinary reasons (no GPS fix, no permission,
/// no stored settings). Reserve exceptions for genuine defects.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("A successful result cannot carry an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("A failed result must carry an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot read the value of a failed result ({Error.Code}).");

    public TValue? ValueOrDefault => _value;
}
