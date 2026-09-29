using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Dua;

public class DuaSource
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public AuthenticityLevel Authenticity { get; set; } = AuthenticityLevel.Unknown;

    public string? Text { get; set; }
}
