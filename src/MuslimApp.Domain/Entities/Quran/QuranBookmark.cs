namespace MuslimApp.Domain.Entities.Quran;

public class QuranBookmark
{
    public int Id { get; set; }

    public int SurahId { get; set; }

    public int AyahNumber { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? Note { get; set; }
}
