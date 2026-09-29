using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MuslimApp.Domain.Entities.Quran;

namespace MuslimApp.Infrastructure.Persistence.Configurations;

internal sealed class QuranSurahConfiguration : IEntityTypeConfiguration<QuranSurah>
{
    public void Configure(EntityTypeBuilder<QuranSurah> builder)
    {
        builder.ToTable("QuranSurahs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ArabicName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.EnglishName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.RevelationType).HasConversion<int>();
        builder.HasIndex(x => x.Number).IsUnique();
    }
}

internal sealed class QuranAyahConfiguration : IEntityTypeConfiguration<QuranAyah>
{
    public void Configure(EntityTypeBuilder<QuranAyah> builder)
    {
        builder.ToTable("QuranAyahs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ArabicText).IsRequired();

        builder.HasOne(x => x.Surah)
            .WithMany(x => x.Ayahs)
            .HasForeignKey(x => x.SurahId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SurahId, x.AyahNumber }).IsUnique();
        builder.HasIndex(x => x.Juz);
        builder.HasIndex(x => x.PageNumber);
    }
}

internal sealed class QuranBookmarkConfiguration : IEntityTypeConfiguration<QuranBookmark>
{
    public void Configure(EntityTypeBuilder<QuranBookmark> builder)
    {
        builder.ToTable("QuranBookmarks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(512);
        builder.HasIndex(x => new { x.SurahId, x.AyahNumber }).IsUnique();
    }
}

internal sealed class QuranReadingProgressConfiguration : IEntityTypeConfiguration<QuranReadingProgress>
{
    public void Configure(EntityTypeBuilder<QuranReadingProgress> builder)
    {
        builder.ToTable("QuranReadingProgress");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
    }
}
