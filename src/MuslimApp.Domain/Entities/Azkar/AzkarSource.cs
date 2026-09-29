using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Azkar;

/// <summary>
/// The narration behind a dhikr. <see cref="Authenticity"/> defaults to
/// <see cref="AuthenticityLevel.Unknown"/> so content that has not been graded is never rendered as
/// authentic.
/// </summary>
public class AzkarSource
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public AuthenticityLevel Authenticity { get; set; } = AuthenticityLevel.Unknown;

    public string? Text { get; set; }
}
