using MuslimApp.Application.DTOs;
using MuslimApp.Application.Mappers;
using MuslimApp.Domain.Common;

namespace MuslimApp.Application.Common;

/// <summary>
/// The selectable calculation conventions, in the order they are offered. Umm al-Qura comes first
/// because the primary audience is in the Gulf; the list is otherwise alphabetical by body.
/// </summary>
public static class CalculationMethodOptions
{
    public static IReadOnlyList<CalculationMethodOptionDto> All { get; } =
        CalculationMethodParameters.All
            .Select(parameters => parameters.ToOptionDto())
            .OrderBy(option => option.Method)
            .ToList();
}
