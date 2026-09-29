using MuslimApp.Domain.Common;

namespace MuslimApp.Domain.Tests;

public class CoordinatesTests
{
    [Theory]
    [InlineData(91d, 0d)]
    [InlineData(-91d, 0d)]
    [InlineData(0d, 181d)]
    [InlineData(0d, -181d)]
    [InlineData(double.NaN, 0d)]
    [InlineData(0d, double.NaN)]
    public void Rejects_positions_that_cannot_exist(double latitude, double longitude)
    {
        Action act = () => _ = new Coordinates(latitude, longitude);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Accepts_the_poles_and_the_antimeridian()
    {
        Action act = () =>
        {
            _ = new Coordinates(90d, 180d);
            _ = new Coordinates(-90d, -180d);
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void TryCreate_reports_failure_instead_of_throwing()
    {
        bool created = Coordinates.TryCreate(200d, 0d, 0d, out Coordinates coordinates);

        created.Should().BeFalse();
        coordinates.Should().Be(default(Coordinates));
    }

    [Fact]
    public void Equality_is_by_value()
    {
        var first = new Coordinates(24.7136d, 46.6753d, 612d);
        var second = new Coordinates(24.7136d, 46.6753d, 612d);

        first.Should().Be(second);
    }
}
