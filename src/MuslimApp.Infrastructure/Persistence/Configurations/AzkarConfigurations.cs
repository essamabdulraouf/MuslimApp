using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MuslimApp.Domain.Entities.Azkar;
using MuslimApp.Domain.Enums;

namespace MuslimApp.Infrastructure.Persistence.Configurations;

internal sealed class AzkarCategoryConfiguration : IEntityTypeConfiguration<AzkarCategory>
{
    public void Configure(EntityTypeBuilder<AzkarCategory> builder)
    {
        builder.ToTable("AzkarCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Description).HasMaxLength(256);
        builder.Property(x => x.Icon).HasMaxLength(64);
        builder.HasIndex(x => x.Type).IsUnique();

        // Only the section structure is seeded. The azkar themselves arrive through a reviewed content
        // package, because every item needs a graded source before it can be shown.
        builder.HasData(
            new AzkarCategory { Id = 1, Type = AzkarCategoryType.Morning, Name = "أذكار الصباح", Icon = "sunrise", Order = 1 },
            new AzkarCategory { Id = 2, Type = AzkarCategoryType.Evening, Name = "أذكار المساء", Icon = "sunset", Order = 2 },
            new AzkarCategory { Id = 3, Type = AzkarCategoryType.AfterPrayer, Name = "أذكار بعد الصلاة", Icon = "mosque", Order = 3 },
            new AzkarCategory { Id = 4, Type = AzkarCategoryType.Sleep, Name = "أذكار النوم", Icon = "moon", Order = 4 },
            new AzkarCategory { Id = 5, Type = AzkarCategoryType.General, Name = "أذكار عامة", Icon = "beads", Order = 5 });
    }
}

internal sealed class AzkarItemConfiguration : IEntityTypeConfiguration<AzkarItem>
{
    public void Configure(EntityTypeBuilder<AzkarItem> builder)
    {
        builder.ToTable("AzkarItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ArabicText).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CategoryId, x.Order });
    }
}

internal sealed class AzkarSourceConfiguration : IEntityTypeConfiguration<AzkarSource>
{
    public void Configure(EntityTypeBuilder<AzkarSource> builder)
    {
        builder.ToTable("AzkarSources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Reference).HasMaxLength(256);
        builder.Property(x => x.Authenticity).HasConversion<int>();
    }
}

internal sealed class DhikrProgressConfiguration : IEntityTypeConfiguration<DhikrProgress>
{
    public void Configure(EntityTypeBuilder<DhikrProgress> builder)
    {
        builder.ToTable("DhikrProgress");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.SessionDate);
        builder.HasIndex(x => new { x.SessionDate, x.DhikrId }).IsUnique();
    }
}
