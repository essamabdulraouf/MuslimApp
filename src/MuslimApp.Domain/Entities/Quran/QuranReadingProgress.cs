namespace MuslimApp.Domain.Entities.Quran;

/// <summary>
/// The single "continue reading" position. Reading history is intentionally not accumulated.
/// </summary>
public class QuranReadingProgress
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public int SurahId { get; set; }

    public int AyahNumber { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
