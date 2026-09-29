using MuslimApp.Domain.Common;

namespace MuslimApp.Domain.Interfaces;

/// <summary>
/// The only way the rest of the application obtains prayer times. Implementations are pure: same
/// inputs, same outputs, no I/O, no ambient clock. That is what makes the timetable testable, and the
/// timetable is the app's core trust promise.
/// </summary>
public interface IPrayerCalculationService
{
    PrayerTimes Calculate(Coordinates coordinates, DateOnly date, PrayerCalculationSettings settings);
}
