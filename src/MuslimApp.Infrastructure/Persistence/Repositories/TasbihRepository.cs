using Microsoft.EntityFrameworkCore;
using MuslimApp.Domain.Entities.Tasbih;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Infrastructure.Persistence.Repositories;

public sealed class TasbihRepository(IDbContextFactory<MuslimDbContext> contextFactory) : ITasbihRepository
{
    public async Task<IReadOnlyList<TasbihPreset>> GetPresetsAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.TasbihPresets
            .AsNoTracking()
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// The most recent session that was never completed, so a count interrupted by the app closing can be
    /// resumed instead of lost.
    /// </summary>
    public async Task<TasbihSession?> GetOpenSessionAsync(CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.TasbihSessions
            .AsNoTracking()
            .Where(x => x.CompletedAt == null)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TasbihSession> StartSessionAsync(int presetId, int targetCount, CancellationToken cancellationToken = default)
    {
        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var session = new TasbihSession
        {
            PresetId = presetId,
            TargetCount = targetCount,
            Count = 0,
            StartedAt = DateTimeOffset.UtcNow
        };

        context.TasbihSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return session;
    }

    public async Task UpdateSessionAsync(TasbihSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        TasbihSession? existing = await context.TasbihSessions
            .FirstOrDefaultAsync(x => x.Id == session.Id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return;
        }

        existing.Count = session.Count;
        existing.TargetCount = session.TargetCount;
        existing.CompletedAt = session.CompletedAt;
        existing.DurationSeconds = session.DurationSeconds;

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
