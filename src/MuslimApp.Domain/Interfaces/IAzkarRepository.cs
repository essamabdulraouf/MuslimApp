using MuslimApp.Domain.Entities.Azkar;

namespace MuslimApp.Domain.Interfaces;

public interface IAzkarRepository
{
    Task<IReadOnlyList<AzkarCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AzkarItem>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<AzkarItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DhikrProgress>> GetProgressForDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task SaveProgressAsync(DhikrProgress progress, CancellationToken cancellationToken = default);
}
