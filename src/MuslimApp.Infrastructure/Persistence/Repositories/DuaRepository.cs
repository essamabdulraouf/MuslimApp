using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Interfaces;
using DuaEntities = MuslimApp.Domain.Entities.Dua;

namespace MuslimApp.Infrastructure.Persistence.Repositories;

public sealed class DuaRepository(IDbContextFactory<MuslimDbContext> contextFactory) : IDuaRepository
{
    public async Task<IReadOnlyList<DuaEntities.DuaCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.DuaCategories
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DuaEntities.Dua>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.Duas
            .AsNoTracking()
            .Include(x => x.Source)
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DuaEntities.Dua>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        string trimmed = query.Trim();

        return await context.Duas
            .AsNoTracking()
            .Include(x => x.Source)
            .Where(x => EF.Functions.Like(x.Title, $"%{trimmed}%") || EF.Functions.Like(x.ArabicText, $"%{trimmed}%"))
            .OrderBy(x => x.CategoryId)
            .ThenBy(x => x.Order)
            .Take(200)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
