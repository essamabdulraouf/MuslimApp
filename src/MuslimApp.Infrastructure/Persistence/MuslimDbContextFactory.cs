using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MuslimApp.Infrastructure.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c> when generating migrations. The path is a throwaway design-time file and
/// is never opened by the app, which builds its connection string from the platform's data directory.
/// </summary>
public sealed class MuslimDbContextFactory : IDesignTimeDbContextFactory<MuslimDbContext>
{
    public MuslimDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<MuslimDbContext> options = new DbContextOptionsBuilder<MuslimDbContext>()
            .UseSqlite("Data Source=MuslimApp.design.db")
            .Options;

        return new MuslimDbContext(options);
    }
}
