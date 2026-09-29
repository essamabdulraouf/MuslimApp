using MuslimApp.Domain.Entities.Tasbih;

namespace MuslimApp.Domain.Interfaces;

public interface ITasbihRepository
{
    Task<IReadOnlyList<TasbihPreset>> GetPresetsAsync(CancellationToken cancellationToken = default);

    Task<TasbihSession?> GetOpenSessionAsync(CancellationToken cancellationToken = default);

    Task<TasbihSession> StartSessionAsync(int presetId, int targetCount, CancellationToken cancellationToken = default);

    Task UpdateSessionAsync(TasbihSession session, CancellationToken cancellationToken = default);
}
