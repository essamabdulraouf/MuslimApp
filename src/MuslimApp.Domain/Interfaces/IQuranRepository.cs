using MuslimApp.Domain.Entities.Quran;

namespace MuslimApp.Domain.Interfaces;

public interface IQuranRepository
{
    Task<IReadOnlyList<QuranSurah>> GetSurahsAsync(CancellationToken cancellationToken = default);

    Task<QuranSurah?> GetSurahAsync(int surahNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuranAyah>> GetAyahsAsync(int surahNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuranAyah>> SearchAsync(string query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuranBookmark>> GetBookmarksAsync(CancellationToken cancellationToken = default);

    Task AddBookmarkAsync(QuranBookmark bookmark, CancellationToken cancellationToken = default);

    Task RemoveBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default);

    Task<QuranReadingProgress?> GetProgressAsync(CancellationToken cancellationToken = default);

    Task SaveProgressAsync(int surahNumber, int ayahNumber, CancellationToken cancellationToken = default);
}
