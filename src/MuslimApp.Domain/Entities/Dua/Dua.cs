namespace MuslimApp.Domain.Entities.Dua;

public class Dua
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ArabicText { get; set; } = string.Empty;

    public string? Translation { get; set; }

    public int? SourceId { get; set; }

    public int Order { get; set; }

    public DuaCategory? Category { get; set; }

    public DuaSource? Source { get; set; }
}
