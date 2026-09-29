using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Entities.Azkar;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Persistence.Repositories;

public sealed class AzkarRepository(IDbContextFactory<MuslimDbContext> contextFactory) : IAzkarRepository
{
    public async Task<IReadOnlyList<AzkarCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.AzkarCategories
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AzkarItem>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // The source is included eagerly: an item must never be rendered without the grading that tells
        // the reader how well attested it is.
        return await context.AzkarItems
            .AsNoTracking()
            .Include(x => x.Source)
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AzkarItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.AzkarItems
            .AsNoTracking()
            .Include(x => x.Source)
            .FirstOrDefaultAsync(x => x.Id == itemId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DhikrProgress>> GetProgressForDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.DhikrProgress
            .AsNoTracking()
            .Where(x => x.SessionDate == date)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task SaveProgressAsync(DhikrProgress progress, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(progress);

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        DhikrProgress? existing = await context.DhikrProgress
            .FirstOrDefaultAsync(
                x => x.SessionDate == progress.SessionDate && x.DhikrId == progress.DhikrId,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            progress.Id = 0;
            context.DhikrProgress.Add(progress);
        }
        else
        {
            existing.CompletedCount = progress.CompletedCount;
            existing.CompletedAt = progress.CompletedAt;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
