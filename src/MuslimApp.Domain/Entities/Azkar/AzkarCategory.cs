using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Azkar;

public class AzkarCategory
{
    public int Id { get; set; }

    public AzkarCategoryType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public int Order { get; set; }

    public ICollection<AzkarItem> Items { get; set; } = new List<AzkarItem>();
}
