namespace MuslimApp.Domain.Entities.Tasbih;

public class TasbihPreset
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ArabicText { get; set; } = string.Empty;

    public int TargetCount { get; set; } = 33;

    public int Order { get; set; }

    public ICollection<TasbihSession> Sessions { get; set; } = new List<TasbihSession>();
}
