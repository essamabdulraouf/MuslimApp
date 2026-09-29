namespace MuslimApp.Domain.Entities.Azkar;

/// <summary>
/// Today's counter state for one dhikr, kept so a session survives the app being closed. Rows are
/// scoped to <see cref="SessionDate"/> and are safe to prune; no long-term history is retained.
/// </summary>
public class DhikrProgress
{
    public int Id { get; set; }

    public int DhikrId { get; set; }

    public int CompletedCount { get; set; }

    public DateOnly SessionDate { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
