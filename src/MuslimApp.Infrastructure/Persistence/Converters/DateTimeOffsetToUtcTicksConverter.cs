using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MuslimApp.Infrastructure.Persistence.Converters;

/// <summary>
/// Stores a <see cref="DateTimeOffset"/> as UTC ticks.
/// </summary>
/// <remarks>
/// SQLite's default mapping writes the value as text including its offset, which cannot be ordered or
/// compared in SQL: "2026-09-29T01:00:00+03:00" sorts after "2026-09-29T00:00:00+00:00" even though it is
/// the earlier instant. Ticks make every comparison and <c>ORDER BY</c> correct in the database.
/// <para>
/// The trade-off is that the original offset is not preserved, so a value read back is expressed in UTC.
/// That is acceptable here because no stored column is the source of a displayed local time: displayed
/// prayer times are recalculated from the user's zone, and the stored columns are instants (when a
/// bookmark was made, when a session started).
/// </para>
/// </remarks>
public sealed class DateTimeOffsetToUtcTicksConverter : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToUtcTicksConverter()
        : base(
            value => value.UtcDateTime.Ticks,
            ticks => new DateTimeOffset(ticks, TimeSpan.Zero))
    {
    }
}
