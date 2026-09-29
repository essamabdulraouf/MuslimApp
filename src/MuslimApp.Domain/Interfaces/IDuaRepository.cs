using MuslimApp.Domain.Entities.Dua;

namespace MuslimApp.Domain.Interfaces;

public interface IDuaRepository
{
    Task<IReadOnlyList<DuaCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dua>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dua>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
