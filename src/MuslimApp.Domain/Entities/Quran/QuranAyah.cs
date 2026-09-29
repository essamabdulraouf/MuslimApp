namespace MuslimApp.Domain.Entities.Quran;

/// <summary>
/// One ayah. <see cref="ArabicText"/> is revealed text: it is written once from a verified content
/// package and never normalized, corrected, or reformatted afterwards. Translation and tafsir are
/// deliberately stored elsewhere so they can never be confused with the original.
/// </summary>
public class QuranAyah
{
    public int Id { get; set; }

    public int SurahId { get; set; }

    public int AyahNumber { get; set; }

    public string ArabicText { get; set; } = string.Empty;

    public int Juz { get; set; }

    public int Hizb { get; set; }

    public int PageNumber { get; set; }

    public bool Sajdah { get; set; }

    public QuranSurah? Surah { get; set; }
}
