using MuslimApp.Domain.Enums;

namespace MuslimApp.Domain.Entities.Dua;

public class DuaCategory
{
    public int Id { get; set; }

    public DuaCategoryType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public int Order { get; set; }

    public ICollection<Dua> Duas { get; set; } = new List<Dua>();
}
