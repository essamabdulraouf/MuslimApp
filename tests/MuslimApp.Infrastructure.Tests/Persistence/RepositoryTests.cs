using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Azkar;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Quran;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Enums;
using MuslimApp.Infrastructure.Persistence.Repositories;

namespace MuslimApp.Infrastructure.Tests.Persistence;

public class SettingsRepositoryTests
{
    [Fact]
    public async Task Reading_settings_for_the_first_time_creates_them()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        AppSettings appSettings = await repository.GetAppSettingsAsync();
        PrayerSettings prayerSettings = await repository.GetPrayerSettingsAsync();
        NotificationSettings notificationSettings = await repository.GetNotificationSettingsAsync();

        appSettings.Id.Should().Be(AppSettings.SingletonId);
        prayerSettings.Id.Should().Be(PrayerSettings.SingletonId);
        notificationSettings.Id.Should().Be(NotificationSettings.SingletonId);
    }

    [Fact]
    public async Task Defaults_put_Arabic_and_Umm_al_Qura_first()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        AppSettings appSettings = await repository.GetAppSettingsAsync();
        PrayerSettings prayerSettings = await repository.GetPrayerSettingsAsync();

        appSettings.Language.Should().Be(Language.Arabic);
        appSettings.Theme.Should().Be(ThemeMode.System);
        appSettings.FirstLaunchCompleted.Should().BeFalse();
        prayerSettings.CalculationMethod.Should().Be(CalculationMethod.UmmAlQura);
        prayerSettings.Madhab.Should().Be(Madhab.Shafi);
    }

    [Fact]
    public async Task A_new_installation_has_no_location_yet()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        PrayerSettings settings = await repository.GetPrayerSettingsAsync();

        settings.HasLocation.Should().BeFalse();
        settings.TimeZoneId.Should().NotBeNullOrWhiteSpace("the engine needs a zone even before a location is chosen");
    }

    [Fact]
    public async Task Reading_twice_does_not_create_a_second_row()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        await repository.GetAppSettingsAsync();
        await repository.GetAppSettingsAsync();

        using MuslimApp.Infrastructure.Persistence.MuslimDbContext context = database.CreateDbContext();
        context.AppSettings.Should().HaveCount(1);
    }

    [Fact]
    public async Task Saving_settings_round_trips_every_field()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        PrayerSettings settings = await repository.GetPrayerSettingsAsync();
        settings.CalculationMethod = CalculationMethod.Karachi;
        settings.Madhab = Madhab.Hanafi;
        settings.HighLatitudeRule = HighLatitudeRule.SeventhOfTheNight;
        settings.Latitude = 24.7136d;
        settings.Longitude = 46.6753d;
        settings.Elevation = 612d;
        settings.LocationName = "الرياض";
        settings.TimeZoneId = "Asia/Riyadh";
        settings.FajrAdjustment = 2;
        settings.IshaAdjustment = -3;
        settings.HasLocation = true;

        await repository.SavePrayerSettingsAsync(settings);
        PrayerSettings reloaded = await repository.GetPrayerSettingsAsync();

        reloaded.CalculationMethod.Should().Be(CalculationMethod.Karachi);
        reloaded.Madhab.Should().Be(Madhab.Hanafi);
        reloaded.HighLatitudeRule.Should().Be(HighLatitudeRule.SeventhOfTheNight);
        reloaded.Latitude.Should().Be(24.7136d);
        reloaded.Longitude.Should().Be(46.6753d);
        reloaded.Elevation.Should().Be(612d);
        reloaded.LocationName.Should().Be("الرياض");
        reloaded.TimeZoneId.Should().Be("Asia/Riyadh");
        reloaded.FajrAdjustment.Should().Be(2);
        reloaded.IshaAdjustment.Should().Be(-3);
        reloaded.HasLocation.Should().BeTrue();
    }

    [Fact]
    public async Task Arabic_text_survives_a_round_trip_unchanged()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        const string name = "مكة المكرمة";
        PrayerSettings settings = await repository.GetPrayerSettingsAsync();
        settings.LocationName = name;
        await repository.SavePrayerSettingsAsync(settings);

        PrayerSettings reloaded = await repository.GetPrayerSettingsAsync();

        reloaded.LocationName.Should().Be(name);
    }

    [Fact]
    public async Task A_do_not_disturb_window_round_trips_as_a_time_of_day()
    {
        using var database = new SqliteTestDatabase();
        var repository = new SettingsRepository(database);

        NotificationSettings settings = await repository.GetNotificationSettingsAsync();
        settings.DoNotDisturbEnabled = true;
        settings.DoNotDisturbFrom = new TimeOnly(23, 15);
        settings.DoNotDisturbTo = new TimeOnly(5, 45);
        await repository.SaveNotificationSettingsAsync(settings);

        NotificationSettings reloaded = await repository.GetNotificationSettingsAsync();

        reloaded.DoNotDisturbFrom.Should().Be(new TimeOnly(23, 15));
        reloaded.DoNotDisturbTo.Should().Be(new TimeOnly(5, 45));
    }
}

public class PrayerTimesRepositoryTests
{
    private static readonly DateTimeOffset Noon = new(2026, 9, 29, 12, 0, 0, TimeSpan.FromHours(3));

    [Fact]
    public async Task A_day_can_be_cached_and_read_back()
    {
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);
        var date = new DateOnly(2026, 9, 29);

        await repository.UpsertAsync(Build(date));
        PrayerTime? stored = await repository.GetAsync(date);

        stored.Should().NotBeNull();
        stored!.Date.Should().Be(date);
        stored.Dhuhr.Should().Be(Noon);
    }

    [Fact]
    public async Task Caching_the_same_day_twice_replaces_it_rather_than_duplicating_it()
    {
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);
        var date = new DateOnly(2026, 9, 29);

        await repository.UpsertAsync(Build(date));
        PrayerTime updated = Build(date);
        updated.Dhuhr = Noon.AddMinutes(1);
        await repository.UpsertAsync(updated);

        IReadOnlyList<PrayerTime> range = await repository.GetRangeAsync(date, date);

        range.Should().HaveCount(1);
        range[0].Dhuhr.Should().Be(Noon.AddMinutes(1));
    }

    [Fact]
    public async Task A_range_comes_back_in_date_order()
    {
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);

        await repository.UpsertAsync(Build(new DateOnly(2026, 9, 30)));
        await repository.UpsertAsync(Build(new DateOnly(2026, 9, 28)));
        await repository.UpsertAsync(Build(new DateOnly(2026, 9, 29)));

        IReadOnlyList<PrayerTime> range = await repository.GetRangeAsync(new DateOnly(2026, 9, 28), new DateOnly(2026, 9, 30));

        range.Select(x => x.Date).Should().BeInAscendingOrder();
        range.Should().HaveCount(3);
    }

    [Fact]
    public async Task Clearing_the_cache_removes_everything()
    {
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);

        await repository.UpsertAsync(Build(new DateOnly(2026, 9, 29)));
        await repository.ClearAsync();

        (await repository.GetAsync(new DateOnly(2026, 9, 29))).Should().BeNull();
    }

    [Fact]
    public async Task A_missing_day_is_reported_as_missing_rather_than_failing()
    {
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);

        (await repository.GetAsync(new DateOnly(2030, 1, 1))).Should().BeNull();
    }

    [Fact]
    public async Task The_instant_survives_a_round_trip_even_though_the_offset_does_not()
    {
        // Stored instants come back in UTC by design: SQLite cannot order an offset-bearing column, so the
        // offset is dropped. Nothing displayed to the user comes from this table.
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);
        var date = new DateOnly(2026, 9, 29);

        await repository.UpsertAsync(Build(date));
        PrayerTime? stored = await repository.GetAsync(date);

        stored!.Dhuhr.UtcDateTime.Should().Be(Noon.UtcDateTime);
        stored.Dhuhr.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Cached_days_can_be_ordered_and_filtered_in_the_database()
    {
        // This is the regression guard for the offset storage problem: ordering by a stored instant must
        // translate to SQL rather than throwing.
        using var database = new SqliteTestDatabase();
        var repository = new PrayerTimesRepository(database);

        await repository.UpsertAsync(Build(new DateOnly(2026, 9, 29)));

        using MuslimApp.Infrastructure.Persistence.MuslimDbContext context = database.CreateDbContext();
        List<PrayerTime> ordered = context.PrayerTimes.OrderByDescending(x => x.Fajr).ToList();

        ordered.Should().HaveCount(1);
    }

    private static PrayerTime Build(DateOnly date) => new()
    {
        Date = date,
        Fajr = Noon.AddHours(-7),
        Sunrise = Noon.AddHours(-6),
        Dhuhr = Noon,
        Asr = Noon.AddHours(3),
        Maghrib = Noon.AddHours(6),
        Isha = Noon.AddHours(7)
    };
}

public class AzkarRepositoryTests
{
    [Fact]
    public async Task The_seeded_sections_are_present_and_ordered()
    {
        using var database = new SqliteTestDatabase();
        var repository = new AzkarRepository(database);

        IReadOnlyList<AzkarCategory> categories = await repository.GetCategoriesAsync();

        categories.Should().HaveCount(5);
        categories.Select(x => x.Order).Should().BeInAscendingOrder();
        categories.Select(x => x.Type).Should().Contain(
        [
            AzkarCategoryType.Morning,
            AzkarCategoryType.Evening,
            AzkarCategoryType.AfterPrayer,
            AzkarCategoryType.Sleep,
            AzkarCategoryType.General
        ]);
    }

    [Fact]
    public async Task No_azkar_are_shipped_until_they_have_been_reviewed()
    {
        using var database = new SqliteTestDatabase();
        var repository = new AzkarRepository(database);

        IReadOnlyList<AzkarItem> items = await repository.GetByCategoryAsync(1);

        items.Should().BeEmpty("content arrives through a reviewed package, not through the schema");
    }

    [Fact]
    public async Task An_ungraded_source_defaults_to_unknown_authenticity()
    {
        using var database = new SqliteTestDatabase();

        using (MuslimApp.Infrastructure.Persistence.MuslimDbContext seed = database.CreateDbContext())
        {
            seed.AzkarSources.Add(new AzkarSource { Id = 1, Name = "رواه مسلم" });
            seed.AzkarItems.Add(new AzkarItem
            {
                Id = 1,
                CategoryId = 1,
                ArabicText = "أَصْبَحْنَا وَأَصْبَحَ الْمُلْكُ لِلَّهِ",
                RepeatCount = 1,
                Order = 1,
                SourceId = 1
            });
            await seed.SaveChangesAsync();
        }

        IReadOnlyList<AzkarItem> items = await new AzkarRepository(database).GetByCategoryAsync(1);

        items.Should().HaveCount(1);
        items[0].Source.Should().NotBeNull("an item must never be shown without its grading");
        items[0].Source!.Authenticity.Should().Be(AuthenticityLevel.Unknown);
    }

    [Fact]
    public async Task Counter_progress_is_kept_per_day_and_per_dhikr()
    {
        using var database = new SqliteTestDatabase();
        var repository = new AzkarRepository(database);
        var today = new DateOnly(2026, 9, 29);

        await repository.SaveProgressAsync(new DhikrProgress { DhikrId = 1, SessionDate = today, CompletedCount = 3 });
        await repository.SaveProgressAsync(new DhikrProgress { DhikrId = 1, SessionDate = today, CompletedCount = 7 });
        await repository.SaveProgressAsync(new DhikrProgress { DhikrId = 2, SessionDate = today, CompletedCount = 1 });
        await repository.SaveProgressAsync(new DhikrProgress { DhikrId = 1, SessionDate = today.AddDays(-1), CompletedCount = 9 });

        IReadOnlyList<DhikrProgress> progress = await repository.GetProgressForDateAsync(today);

        progress.Should().HaveCount(2);
        progress.Single(x => x.DhikrId == 1).CompletedCount.Should().Be(7, "the same dhikr on the same day is updated, not duplicated");
    }
}

public class QuranRepositoryTests
{
    [Fact]
    public async Task An_empty_database_returns_no_surahs_rather_than_failing()
    {
        using var database = new SqliteTestDatabase();
        var repository = new QuranRepository(database);

        (await repository.GetSurahsAsync()).Should().BeEmpty();
        (await repository.GetSurahAsync(1)).Should().BeNull();
        (await repository.SearchAsync("الله")).Should().BeEmpty();
    }

    [Fact]
    public async Task A_blank_search_returns_nothing_without_touching_the_database()
    {
        using var database = new SqliteTestDatabase();
        var repository = new QuranRepository(database);

        (await repository.SearchAsync("   ")).Should().BeEmpty();
    }

    [Fact]
    public async Task Ayah_text_is_stored_and_returned_byte_for_byte()
    {
        // Quranic text is immutable: whatever the content package provides must come back identical,
        // including every diacritic.
        const string basmala = "بِسْمِ ٱللَّهِ ٱلرَّحْمَٰنِ ٱلرَّحِيمِ";

        using var database = new SqliteTestDatabase();

        using (MuslimApp.Infrastructure.Persistence.MuslimDbContext seed = database.CreateDbContext())
        {
            seed.QuranSurahs.Add(new QuranSurah
            {
                Id = 1,
                Number = 1,
                ArabicName = "الفاتحة",
                EnglishName = "Al-Fatihah",
                AyahCount = 7,
                RevelationType = RevelationType.Meccan,
                JuzStart = 1,
                PageStart = 1
            });
            seed.QuranAyahs.Add(new QuranAyah
            {
                Id = 1,
                SurahId = 1,
                AyahNumber = 1,
                ArabicText = basmala,
                Juz = 1,
                Hizb = 1,
                PageNumber = 1
            });
            await seed.SaveChangesAsync();
        }

        var repository = new QuranRepository(database);
        IReadOnlyList<QuranAyah> ayahs = await repository.GetAyahsAsync(1);

        ayahs.Should().HaveCount(1);
        ayahs[0].ArabicText.Should().Be(basmala);
        ayahs[0].ArabicText.Should().HaveLength(basmala.Length);
    }

    [Fact]
    public async Task Bookmarking_the_same_ayah_twice_does_not_duplicate_it()
    {
        using var database = new SqliteTestDatabase();
        var repository = new QuranRepository(database);

        await repository.AddBookmarkAsync(new QuranBookmark { SurahId = 2, AyahNumber = 255, CreatedAt = DateTimeOffset.UtcNow });
        await repository.AddBookmarkAsync(new QuranBookmark { SurahId = 2, AyahNumber = 255, CreatedAt = DateTimeOffset.UtcNow });

        (await repository.GetBookmarksAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task Reading_progress_is_a_single_position_not_a_history()
    {
        using var database = new SqliteTestDatabase();
        var repository = new QuranRepository(database);

        await repository.SaveProgressAsync(2, 100);
        await repository.SaveProgressAsync(18, 24);

        QuranReadingProgress? progress = await repository.GetProgressAsync();

        progress.Should().NotBeNull();
        progress!.SurahId.Should().Be(18);
        progress.AyahNumber.Should().Be(24);

        using MuslimApp.Infrastructure.Persistence.MuslimDbContext context = database.CreateDbContext();
        context.QuranReadingProgress.Should().HaveCount(1, "reading history is deliberately not accumulated");
    }
}

public class TasbihRepositoryTests
{
    [Fact]
    public async Task The_seeded_presets_are_the_tasbih_after_the_prayer()
    {
        using var database = new SqliteTestDatabase();
        var repository = new TasbihRepository(database);

        IReadOnlyList<Domain.Entities.Tasbih.TasbihPreset> presets = await repository.GetPresetsAsync();

        presets.Should().HaveCount(5);
        presets.Select(x => x.Order).Should().BeInAscendingOrder();
        presets.Take(3).Should().OnlyContain(x => x.TargetCount == 33);
    }

    [Fact]
    public async Task An_interrupted_session_can_be_resumed()
    {
        using var database = new SqliteTestDatabase();
        var repository = new TasbihRepository(database);

        Domain.Entities.Tasbih.TasbihSession started = await repository.StartSessionAsync(presetId: 1, targetCount: 33);
        started.Count = 12;
        await repository.UpdateSessionAsync(started);

        Domain.Entities.Tasbih.TasbihSession? open = await repository.GetOpenSessionAsync();

        open.Should().NotBeNull();
        open!.Count.Should().Be(12);
        open.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task A_finished_session_is_no_longer_open()
    {
        using var database = new SqliteTestDatabase();
        var repository = new TasbihRepository(database);

        Domain.Entities.Tasbih.TasbihSession started = await repository.StartSessionAsync(presetId: 1, targetCount: 33);
        started.Count = 33;
        started.CompletedAt = DateTimeOffset.UtcNow;
        await repository.UpdateSessionAsync(started);

        (await repository.GetOpenSessionAsync()).Should().BeNull();
    }
}

public class DuaRepositoryTests
{
    [Fact]
    public async Task The_seeded_dua_sections_cover_every_category()
    {
        using var database = new SqliteTestDatabase();
        var repository = new DuaRepository(database);

        IReadOnlyList<Domain.Entities.Dua.DuaCategory> categories = await repository.GetCategoriesAsync();

        categories.Should().HaveCount(Enum.GetValues<DuaCategoryType>().Length);
        categories.Select(x => x.Order).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task No_duas_are_shipped_until_they_have_been_reviewed()
    {
        using var database = new SqliteTestDatabase();
        var repository = new DuaRepository(database);

        (await repository.GetByCategoryAsync(1)).Should().BeEmpty();
        (await repository.SearchAsync("رب")).Should().BeEmpty();
    }
}
