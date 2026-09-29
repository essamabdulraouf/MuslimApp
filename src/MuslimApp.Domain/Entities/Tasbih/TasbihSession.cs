namespace MuslimApp.Domain.Entities.Tasbih;

/// <summary>
/// One counting session. <see cref="CompletedAt"/> stays null while the session is open, which is how
/// an interrupted count is resumed.
/// </summary>
public class TasbihSession
{
    public int Id { get; set; }

    public int PresetId { get; set; }

    public int Count { get; set; }

    public int TargetCount { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public int DurationSeconds { get; set; }

    public TasbihPreset? Preset { get; set; }

    public bool IsComplete => TargetCount > 0 && Count >= TargetCount;
}
