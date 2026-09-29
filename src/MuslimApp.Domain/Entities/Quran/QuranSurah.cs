using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Quran;

/// <summary>
/// Metadata for one surah. The Arabic name is revealed content and is treated as immutable.
/// </summary>
public class QuranSurah
{
    public int Id { get; set; }

    public int Number { get; set; }

    public string ArabicName { get; set; } = string.Empty;

    public string EnglishName { get; set; } = string.Empty;

    public int AyahCount { get; set; }

    public RevelationType RevelationType { get; set; }

    public int JuzStart { get; set; }

    public int PageStart { get; set; }

    public ICollection<QuranAyah> Ayahs { get; set; } = new List<QuranAyah>();
}
