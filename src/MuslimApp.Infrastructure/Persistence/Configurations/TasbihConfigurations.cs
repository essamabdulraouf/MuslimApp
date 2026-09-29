using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MuslimApp.Domain.Entities.Tasbih;

namespace MuslimApp.Infrastructure.Persistence.Configurations;

internal sealed class TasbihPresetConfiguration : IEntityTypeConfiguration<TasbihPreset>
{
    public void Configure(EntityTypeBuilder<TasbihPreset> builder)
    {
        builder.ToTable("TasbihPresets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ArabicText).IsRequired().HasMaxLength(256);

        // The tasbih following the prayer, as reported in Sahih Muslim.
        builder.HasData(
            new TasbihPreset { Id = 1, Name = "سبحان الله", ArabicText = "سُبْحَانَ اللَّهِ", TargetCount = 33, Order = 1 },
            new TasbihPreset { Id = 2, Name = "الحمد لله", ArabicText = "الْحَمْدُ لِلَّهِ", TargetCount = 33, Order = 2 },
            new TasbihPreset { Id = 3, Name = "الله أكبر", ArabicText = "اللَّهُ أَكْبَرُ", TargetCount = 33, Order = 3 },
            new TasbihPreset { Id = 4, Name = "لا إله إلا الله", ArabicText = "لَا إِلَهَ إِلَّا اللَّهُ", TargetCount = 100, Order = 4 },
            new TasbihPreset { Id = 5, Name = "أستغفر الله", ArabicText = "أَسْتَغْفِرُ اللَّهَ", TargetCount = 100, Order = 5 });
    }
}

internal sealed class TasbihSessionConfiguration : IEntityTypeConfiguration<TasbihSession>
{
    public void Configure(EntityTypeBuilder<TasbihSession> builder)
    {
        builder.ToTable("TasbihSessions");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Preset)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.PresetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StartedAt);
        builder.Ignore(x => x.IsComplete);
    }
}
