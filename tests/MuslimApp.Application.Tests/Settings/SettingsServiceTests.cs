using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MuslimApp.Application.Abstractions.Services;
using MuslimApp.Application.Features.Settings;
using MuslimApp.Domain.Common;
using MuslimApp.Domain.Entities.Prayer;
using MuslimApp.Domain.Entities.Settings;
using MuslimApp.Domain.Enums;
using MuslimApp.Domain.Interfaces;

namespace MuslimApp.Application.Tests.Settings;

public class SettingsServiceTests
{
    private readonly Mock<ISettingsRepository> _repository = new();
    private readonly AppSettings _appSettings = new() { Id = AppSettings.SingletonId };
    private readonly PrayerSettings _prayerSettings = new() { Id = PrayerSettings.SingletonId };

    public SettingsServiceTests()
    {
        _repository.Setup(x => x.GetAppSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_appSettings);
        _repository.Setup(x => x.GetPrayerSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_prayerSettings);
        _repository.Setup(x => x.GetNotificationSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationSettings { Id = NotificationSettings.SingletonId });
    }

    [Fact]
    public async Task Choosing_a_location_stores_it_and_marks_the_app_as_configured()
    {
        SettingsService service = Build();
        var location = new LocationResult(
            new Coordinates(24.7136d, 46.6753d, 612d),
            LocationSource.ManualSelection,
            DateTimeOffset.UtcNow,
            LocationName: "الرياض",
            TimeZoneId: "Asia/Riyadh");

        await service.SetLocationAsync(location);

        _prayerSettings.Latitude.Should().Be(24.7136d);
        _prayerSettings.Longitude.Should().Be(46.6753d);
        _prayerSettings.Elevation.Should().Be(612d);
        _prayerSettings.LocationName.Should().Be("الرياض");
        _prayerSettings.TimeZoneId.Should().Be("Asia/Riyadh");
        _prayerSettings.HasLocation.Should().BeTrue();
        _repository.Verify(x => x.SavePrayerSettingsAsync(_prayerSettings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Choosing_a_location_announces_that_the_timetable_is_stale()
    {
        SettingsService service = Build();
        SettingsChangeKind captured = SettingsChangeKind.None;
        service.SettingsChanged += (_, args) => captured = args.Kind;

        await service.SetLocationAsync(new LocationResult(
            new Coordinates(24.7136d, 46.6753d),
            LocationSource.Device,
            DateTimeOffset.UtcNow));

        captured.Should().HaveFlag(SettingsChangeKind.Location);
        captured.Should().HaveFlag(SettingsChangeKind.PrayerCalculation);
    }

    [Fact]
    public async Task A_location_without_a_name_keeps_the_previous_one()
    {
        _prayerSettings.LocationName = "الرياض";
        _prayerSettings.TimeZoneId = "Asia/Riyadh";
        SettingsService service = Build();

        await service.SetLocationAsync(new LocationResult(
            new Coordinates(24.8d, 46.7d),
            LocationSource.Device,
            DateTimeOffset.UtcNow));

        _prayerSettings.LocationName.Should().Be("الرياض");
        _prayerSettings.TimeZoneId.Should().Be("Asia/Riyadh");
    }

    [Fact]
    public async Task Changing_the_calculation_method_announces_a_recalculation()
    {
        SettingsService service = Build();
        SettingsChangeKind captured = SettingsChangeKind.None;
        service.SettingsChanged += (_, args) => captured = args.Kind;

        await service.SetCalculationMethodAsync(CalculationMethod.Karachi);

        _prayerSettings.CalculationMethod.Should().Be(CalculationMethod.Karachi);
        captured.Should().Be(SettingsChangeKind.PrayerCalculation);
    }

    [Fact]
    public async Task Setting_a_value_it_already_has_changes_nothing_and_announces_nothing()
    {
        _prayerSettings.CalculationMethod = CalculationMethod.UmmAlQura;
        SettingsService service = Build();
        var raised = false;
        service.SettingsChanged += (_, _) => raised = true;

        await service.SetCalculationMethodAsync(CalculationMethod.UmmAlQura);

        raised.Should().BeFalse();
        _repository.Verify(x => x.SavePrayerSettingsAsync(It.IsAny<PrayerSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Adjustments_are_written_to_every_prayer()
    {
        SettingsService service = Build();

        await service.SetAdjustmentsAsync(new MinuteAdjustments(1, 2, 3, 4, 5, 6));

        _prayerSettings.FajrAdjustment.Should().Be(1);
        _prayerSettings.SunriseAdjustment.Should().Be(2);
        _prayerSettings.DhuhrAdjustment.Should().Be(3);
        _prayerSettings.AsrAdjustment.Should().Be(4);
        _prayerSettings.MaghribAdjustment.Should().Be(5);
        _prayerSettings.IshaAdjustment.Should().Be(6);
    }

    [Fact]
    public async Task Switching_language_announces_a_presentation_change()
    {
        SettingsService service = Build();
        SettingsChangeKind captured = SettingsChangeKind.None;
        service.SettingsChanged += (_, args) => captured = args.Kind;

        await service.SetLanguageAsync(Language.English);

        _appSettings.Language.Should().Be(Language.English);
        captured.Should().Be(SettingsChangeKind.Presentation);
    }

    [Fact]
    public async Task Completing_onboarding_is_recorded_once()
    {
        SettingsService service = Build();

        await service.CompleteOnboardingAsync();
        await service.CompleteOnboardingAsync();

        _appSettings.FirstLaunchCompleted.Should().BeTrue();
        _repository.Verify(x => x.SaveAppSettingsAsync(_appSettings, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Declining_a_permission_is_remembered_so_the_user_is_not_asked_again()
    {
        SettingsService service = Build();

        await service.MarkLocationPermissionRequestedAsync();
        await service.MarkNotificationPermissionRequestedAsync();

        _appSettings.LocationPermissionRequested.Should().BeTrue();
        _appSettings.NotificationPermissionRequested.Should().BeTrue();
    }

    private SettingsService Build() => new(
        _repository.Object,
        new FixedDateTimeProvider(new DateTimeOffset(2026, 9, 29, 12, 0, 0, TimeSpan.Zero)),
        NullLogger<SettingsService>.Instance);
}
