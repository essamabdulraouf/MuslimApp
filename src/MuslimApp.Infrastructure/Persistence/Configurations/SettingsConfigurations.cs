using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;

namespace MuslimApp.Infrastructure.Persistence.Configurations;

internal sealed class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.ToTable("AppSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Language).HasConversion<int>();
        builder.Property(x => x.Theme).HasConversion<int>();
        builder.Property(x => x.ContentVersion).HasMaxLength(64);
    }
}

internal sealed class NotificationSettingsConfiguration : IEntityTypeConfiguration<NotificationSettings>
{
    public void Configure(EntityTypeBuilder<NotificationSettings> builder)
    {
        builder.ToTable("NotificationSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.AdhanSoundId).HasMaxLength(64);
        builder.Ignore(x => x.DoNotDisturbCrossesMidnight);
    }
}

internal sealed class PrayerSettingsConfiguration : IEntityTypeConfiguration<PrayerSettings>
{
    public void Configure(EntityTypeBuilder<PrayerSettings> builder)
    {
        builder.ToTable("PrayerSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CalculationMethod).HasConversion<int>();
        builder.Property(x => x.Madhab).HasConversion<int>();
        builder.Property(x => x.HighLatitudeRule).HasConversion<int>();
        builder.Property(x => x.LocationName).HasMaxLength(128);
        builder.Property(x => x.TimeZoneId).HasMaxLength(64);
    }
}

internal sealed class PrayerTimeConfiguration : IEntityTypeConfiguration<PrayerTime>
{
    public void Configure(EntityTypeBuilder<PrayerTime> builder)
    {
        builder.ToTable("PrayerTimes");
        builder.HasKey(x => x.Id);

        // One cached row per day; the unique index is what makes the upsert safe.
        builder.HasIndex(x => x.Date).IsUnique();
    }
}
