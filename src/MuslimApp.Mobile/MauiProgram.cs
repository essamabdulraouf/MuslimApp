using Microsoft.Extensions.Logging;
using MuslimApp.Application.DependencyInjection;
using MuslimApp.Infrastructure.DependencyInjection;
using MuslimApp.Mobile.Services;

namespace MuslimApp.Mobile;

public static class MauiProgram
{
    /// <summary>The SQLite file, kept in the platform's private application data directory.</summary>
    private const string DatabaseFileName = "muslimapp.db";

    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Tajawal carries the interface, Amiri is reserved for Quranic text, and the Material faces
                // provide every icon in the app. OpenSans is the Latin fallback for English.
                fonts.AddFont("Tajawal-Regular.ttf", "Tajawal");
                fonts.AddFont("Tajawal-Medium.ttf", "TajawalMedium");
                fonts.AddFont("Tajawal-Bold.ttf", "TajawalBold");
                fonts.AddFont("Amiri-Regular.ttf", "Amiri");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIconsOutlined");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSans");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddApplication()
            .AddInfrastructure(options => options.DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName))
            .AddMobileServices();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
        // Release builds log warnings and above only. Nothing in this app logs a coordinate, a reading position
        // or any other personal detail, and a quieter default keeps it that way by leaving less room to.
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif

        return builder.Build();
    }
}
