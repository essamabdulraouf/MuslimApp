using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MuslimApp.Infrastructure.Persistence;

/// <summary>
/// Brings the local database up to date at startup.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Applies any pending migrations. Returns false if the database could not be prepared, so the caller
    /// can show an error state instead of the app dying on a black screen.
    /// </summary>
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
}

public sealed class DatabaseInitializer(
    IDbContextFactory<MuslimDbContext> contextFactory,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using MuslimDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            IEnumerable<string> pending = await context.Database
                .GetPendingMigrationsAsync(cancellationToken)
                .ConfigureAwait(false);

            var pendingList = pending.ToList();
            if (pendingList.Count == 0)
            {
                logger.LogInformation("The local database is already up to date.");
                return true;
            }

            logger.LogInformation("Applying {Count} pending database migration(s).", pendingList.Count);
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A failed migration is recoverable from the user's point of view: the app can still show a
            // clear error and keep the existing file intact for diagnosis.
            logger.LogError(ex, "The local database could not be prepared.");
            return false;
        }
    }
}
