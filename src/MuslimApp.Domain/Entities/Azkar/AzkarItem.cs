namespace MuslimApp.Domain.Entities.Azkar;

/// <summary>
/// One dhikr. A missing <see cref="SourceId"/> is a content defect: every item must be traceable to
/// a narration whose grading the UI can show.
/// </summary>
public class AzkarItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string ArabicText { get; set; } = string.Empty;

    public int RepeatCount { get; set; } = 1;

    public int Order { get; set; }

    public int? SourceId { get; set; }

    public string? Translation { get; set; }

    public string? Explanation { get; set; }

    public AzkarCategory? Category { get; set; }

    public AzkarSource? Source { get; set; }
}
