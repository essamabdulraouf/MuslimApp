using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Entities.Quran;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Persistence.Repositories;

public sealed class QuranRepository(IDbContextFactory<MuslimDbContext> contextFactory) : IQuranRepository
{
    public async Task<IReadOnlyList<QuranSurah>> GetSurahsAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.QuranSurahs
            .AsNoTracking()
            .OrderBy(x => x.Number)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<QuranSurah?> GetSurahAsync(int surahNumber, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.QuranSurahs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Number == surahNumber, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuranAyah>> GetAyahsAsync(int surahNumber, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.QuranAyahs
            .AsNoTracking()
            .Where(x => x.Surah!.Number == surahNumber)
            .OrderBy(x => x.AyahNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Substring search over the Arabic text. Diacritic-insensitive matching needs a normalized search
    /// column, which is built with the Quran content package rather than by transforming the stored
    /// text.
    /// </summary>
    public async Task<IReadOnlyList<QuranAyah>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        string trimmed = query.Trim();

        return await context.QuranAyahs
            .AsNoTracking()
            .Where(x => EF.Functions.Like(x.ArabicText, $"%{trimmed}%"))
            .OrderBy(x => x.SurahId)
            .ThenBy(x => x.AyahNumber)
            .Take(200)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuranBookmark>> GetBookmarksAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.QuranBookmarks
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddBookmarkAsync(QuranBookmark bookmark, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bookmark);

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        bool exists = await context.QuranBookmarks
            .AnyAsync(x => x.SurahId == bookmark.SurahId && x.AyahNumber == bookmark.AyahNumber, cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            return;
        }

        bookmark.Id = 0;
        context.QuranBookmarks.Add(bookmark);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        await context.QuranBookmarks
            .Where(x => x.Id == bookmarkId)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<QuranReadingProgress?> GetProgressAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.QuranReadingProgress
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == QuranReadingProgress.SingletonId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Overwrites the single "continue reading" position. Reading history is deliberately not kept.
    /// </summary>
    public async Task SaveProgressAsync(int surahNumber, int ayahNumber, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        QuranReadingProgress? progress = await context.QuranReadingProgress
            .FirstOrDefaultAsync(x => x.Id == QuranReadingProgress.SingletonId, cancellationToken)
            .ConfigureAwait(false);

        if (progress is null)
        {
            context.QuranReadingProgress.Add(new QuranReadingProgress
            {
                Id = QuranReadingProgress.SingletonId,
                SurahId = surahNumber,
                AyahNumber = ayahNumber,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            progress.SurahId = surahNumber;
            progress.AyahNumber = ayahNumber;
            progress.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
