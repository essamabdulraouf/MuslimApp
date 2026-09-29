using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MuslimApp.Domain.Enums;
using DuaEntities = MuslimApp.Domain.Entities.Dua;

namespace MuslimApp.Infrastructure.Persistence.Configurations;

internal sealed class DuaCategoryConfiguration : IEntityTypeConfiguration<DuaEntities.DuaCategory>
{
    public void Configure(EntityTypeBuilder<DuaEntities.DuaCategory> builder)
    {
        builder.ToTable("DuaCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Description).HasMaxLength(256);
        builder.Property(x => x.Icon).HasMaxLength(64);
        builder.HasIndex(x => x.Type).IsUnique();

        builder.HasData(
            new DuaEntities.DuaCategory { Id = 1, Type = DuaCategoryType.Quranic, Name = "أدعية من القرآن الكريم", Icon = "book", Order = 1 },
            new DuaEntities.DuaCategory { Id = 2, Type = DuaCategoryType.Prophetic, Name = "أدعية نبوية", Icon = "mosque", Order = 2 },
            new DuaEntities.DuaCategory { Id = 3, Type = DuaCategoryType.Morning, Name = "أدعية الصباح", Icon = "sunrise", Order = 3 },
            new DuaEntities.DuaCategory { Id = 4, Type = DuaCategoryType.Evening, Name = "أدعية المساء", Icon = "sunset", Order = 4 },
            new DuaEntities.DuaCategory { Id = 5, Type = DuaCategoryType.Travel, Name = "أدعية السفر", Icon = "travel", Order = 5 },
            new DuaEntities.DuaCategory { Id = 6, Type = DuaCategoryType.Family, Name = "أدعية الأهل والأبناء", Icon = "family", Order = 6 },
            new DuaEntities.DuaCategory { Id = 7, Type = DuaCategoryType.Health, Name = "أدعية الشفاء والعافية", Icon = "health", Order = 7 },
            new DuaEntities.DuaCategory { Id = 8, Type = DuaCategoryType.Provision, Name = "أدعية الرزق", Icon = "provision", Order = 8 },
            new DuaEntities.DuaCategory { Id = 9, Type = DuaCategoryType.Forgiveness, Name = "أدعية الاستغفار والتوبة", Icon = "forgiveness", Order = 9 },
            new DuaEntities.DuaCategory { Id = 10, Type = DuaCategoryType.General, Name = "أدعية متنوعة", Icon = "general", Order = 10 });
    }
}

internal sealed class DuaConfiguration : IEntityTypeConfiguration<DuaEntities.Dua>
{
    public void Configure(EntityTypeBuilder<DuaEntities.Dua> builder)
    {
        builder.ToTable("Duas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.ArabicText).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Duas)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CategoryId, x.Order });
    }
}

internal sealed class DuaSourceConfiguration : IEntityTypeConfiguration<DuaEntities.DuaSource>
{
    public void Configure(EntityTypeBuilder<DuaEntities.DuaSource> builder)
    {
        builder.ToTable("DuaSources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Reference).HasMaxLength(256);
        builder.Property(x => x.Authenticity).HasConversion<int>();
    }
}
