using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MuslimApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    Theme = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstLaunchCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocationPermissionRequested = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationPermissionRequested = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContentSchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentVersion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzkarCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzkarCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzkarSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Authenticity = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzkarSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DhikrProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DhikrId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DhikrProgress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuaCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuaCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DuaSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Reference = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Authenticity = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuaSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PrayerNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    AdhanEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrayerReminderEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrayerReminderMinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    MorningAzkarEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    EveningAzkarEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AfterPrayerAzkarEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TasbihReminderEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoNotDisturbEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoNotDisturbFrom = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    DoNotDisturbTo = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    AdhanSoundId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrayerSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculationMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Madhab = table.Column<int>(type: "INTEGER", nullable: false),
                    HighLatitudeRule = table.Column<int>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Elevation = table.Column<double>(type: "REAL", nullable: false),
                    LocationName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    FajrAdjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    SunriseAdjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    DhuhrAdjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    AsrAdjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    MaghribAdjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    IshaAdjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomFajrAngle = table.Column<double>(type: "REAL", nullable: true),
                    CustomIshaAngle = table.Column<double>(type: "REAL", nullable: true),
                    CustomIshaIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    HasLocation = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrayerSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrayerTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Fajr = table.Column<long>(type: "INTEGER", nullable: false),
                    Sunrise = table.Column<long>(type: "INTEGER", nullable: false),
                    Dhuhr = table.Column<long>(type: "INTEGER", nullable: false),
                    Asr = table.Column<long>(type: "INTEGER", nullable: false),
                    Maghrib = table.Column<long>(type: "INTEGER", nullable: false),
                    Isha = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrayerTimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuranBookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SurahId = table.Column<int>(type: "INTEGER", nullable: false),
                    AyahNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuranBookmarks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuranReadingProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    SurahId = table.Column<int>(type: "INTEGER", nullable: false),
                    AyahNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuranReadingProgress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuranSurahs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    ArabicName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EnglishName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    AyahCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RevelationType = table.Column<int>(type: "INTEGER", nullable: false),
                    JuzStart = table.Column<int>(type: "INTEGER", nullable: false),
                    PageStart = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuranSurahs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TasbihPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ArabicText = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TargetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasbihPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AzkarItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ArabicText = table.Column<string>(type: "TEXT", nullable: false),
                    RepeatCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    Translation = table.Column<string>(type: "TEXT", nullable: true),
                    Explanation = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AzkarItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AzkarItems_AzkarCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AzkarCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AzkarItems_AzkarSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "AzkarSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Duas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ArabicText = table.Column<string>(type: "TEXT", nullable: false),
                    Translation = table.Column<string>(type: "TEXT", nullable: true),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Duas_DuaCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "DuaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Duas_DuaSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "DuaSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuranAyahs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    SurahId = table.Column<int>(type: "INTEGER", nullable: false),
                    AyahNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ArabicText = table.Column<string>(type: "TEXT", nullable: false),
                    Juz = table.Column<int>(type: "INTEGER", nullable: false),
                    Hizb = table.Column<int>(type: "INTEGER", nullable: false),
                    PageNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Sajdah = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuranAyahs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuranAyahs_QuranSurahs_SurahId",
                        column: x => x.SurahId,
                        principalTable: "QuranSurahs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TasbihSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PresetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasbihSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TasbihSessions_TasbihPresets_PresetId",
                        column: x => x.PresetId,
                        principalTable: "TasbihPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AzkarCategories",
                columns: new[] { "Id", "Description", "Icon", "Name", "Order", "Type" },
                values: new object[,]
                {
                    { 1, null, "sunrise", "أذكار الصباح", 1, 0 },
                    { 2, null, "sunset", "أذكار المساء", 2, 1 },
                    { 3, null, "mosque", "أذكار بعد الصلاة", 3, 2 },
                    { 4, null, "moon", "أذكار النوم", 4, 3 },
                    { 5, null, "beads", "أذكار عامة", 5, 4 }
                });

            migrationBuilder.InsertData(
                table: "DuaCategories",
                columns: new[] { "Id", "Description", "Icon", "Name", "Order", "Type" },
                values: new object[,]
                {
                    { 1, null, "book", "أدعية من القرآن الكريم", 1, 0 },
                    { 2, null, "mosque", "أدعية نبوية", 2, 1 },
                    { 3, null, "sunrise", "أدعية الصباح", 3, 2 },
                    { 4, null, "sunset", "أدعية المساء", 4, 3 },
                    { 5, null, "travel", "أدعية السفر", 5, 4 },
                    { 6, null, "family", "أدعية الأهل والأبناء", 6, 5 },
                    { 7, null, "health", "أدعية الشفاء والعافية", 7, 6 },
                    { 8, null, "provision", "أدعية الرزق", 8, 7 },
                    { 9, null, "forgiveness", "أدعية الاستغفار والتوبة", 9, 8 },
                    { 10, null, "general", "أدعية متنوعة", 10, 9 }
                });

            migrationBuilder.InsertData(
                table: "TasbihPresets",
                columns: new[] { "Id", "ArabicText", "Name", "Order", "TargetCount" },
                values: new object[,]
                {
                    { 1, "سُبْحَانَ اللَّهِ", "سبحان الله", 1, 33 },
                    { 2, "الْحَمْدُ لِلَّهِ", "الحمد لله", 2, 33 },
                    { 3, "اللَّهُ أَكْبَرُ", "الله أكبر", 3, 33 },
                    { 4, "لَا إِلَهَ إِلَّا اللَّهُ", "لا إله إلا الله", 4, 100 },
                    { 5, "أَسْتَغْفِرُ اللَّهَ", "أستغفر الله", 5, 100 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AzkarCategories_Type",
                table: "AzkarCategories",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AzkarItems_CategoryId_Order",
                table: "AzkarItems",
                columns: new[] { "CategoryId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AzkarItems_SourceId",
                table: "AzkarItems",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_DhikrProgress_SessionDate",
                table: "DhikrProgress",
                column: "SessionDate");

            migrationBuilder.CreateIndex(
                name: "IX_DhikrProgress_SessionDate_DhikrId",
                table: "DhikrProgress",
                columns: new[] { "SessionDate", "DhikrId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuaCategories_Type",
                table: "DuaCategories",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Duas_CategoryId_Order",
                table: "Duas",
                columns: new[] { "CategoryId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Duas_SourceId",
                table: "Duas",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrayerTimes_Date",
                table: "PrayerTimes",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuranAyahs_Juz",
                table: "QuranAyahs",
                column: "Juz");

            migrationBuilder.CreateIndex(
                name: "IX_QuranAyahs_PageNumber",
                table: "QuranAyahs",
                column: "PageNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QuranAyahs_SurahId_AyahNumber",
                table: "QuranAyahs",
                columns: new[] { "SurahId", "AyahNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuranBookmarks_SurahId_AyahNumber",
                table: "QuranBookmarks",
                columns: new[] { "SurahId", "AyahNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuranSurahs_Number",
                table: "QuranSurahs",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TasbihSessions_PresetId",
                table: "TasbihSessions",
                column: "PresetId");

            migrationBuilder.CreateIndex(
                name: "IX_TasbihSessions_StartedAt",
                table: "TasbihSessions",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AzkarItems");

            migrationBuilder.DropTable(
                name: "DhikrProgress");

            migrationBuilder.DropTable(
                name: "Duas");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "PrayerSettings");

            migrationBuilder.DropTable(
                name: "PrayerTimes");

            migrationBuilder.DropTable(
                name: "QuranAyahs");

            migrationBuilder.DropTable(
                name: "QuranBookmarks");

            migrationBuilder.DropTable(
                name: "QuranReadingProgress");

            migrationBuilder.DropTable(
                name: "TasbihSessions");

            migrationBuilder.DropTable(
                name: "AzkarCategories");

            migrationBuilder.DropTable(
                name: "AzkarSources");

            migrationBuilder.DropTable(
                name: "DuaCategories");

            migrationBuilder.DropTable(
                name: "DuaSources");

            migrationBuilder.DropTable(
                name: "QuranSurahs");

            migrationBuilder.DropTable(
                name: "TasbihPresets");
        }
    }
}
