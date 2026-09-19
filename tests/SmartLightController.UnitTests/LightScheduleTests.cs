using SmartLightController.Models;

namespace SmartLightController.UnitTests;

public sealed class LightScheduleTests
{
    [Theory]
    [InlineData(8, 0, true)]
    [InlineData(11, 59, true)]
    [InlineData(12, 0, false)]
    [InlineData(7, 59, false)]
    public void SameDaySchedule_UsesInclusiveStartAndExclusiveEnd(int hour, int minute, bool expected)
    {
        var schedule = new LightSchedule(new TimeOnly(8, 0), new TimeOnly(12, 0));

        Assert.Equal(expected, schedule.IsActiveAt(new TimeOnly(hour, minute)));
    }

    [Theory]
    [InlineData(20, 0, true)]
    [InlineData(23, 59, true)]
    [InlineData(0, 0, true)]
    [InlineData(5, 59, true)]
    [InlineData(6, 0, false)]
    [InlineData(12, 0, false)]
    public void OvernightSchedule_SpansMidnight(int hour, int minute, bool expected)
    {
        var schedule = new LightSchedule(new TimeOnly(20, 0), new TimeOnly(6, 0));

        Assert.Equal(expected, schedule.IsActiveAt(new TimeOnly(hour, minute)));
    }

    [Fact]
    public void EqualStartAndEnd_DefinesAnEmptySchedule()
    {
        var schedule = new LightSchedule(new TimeOnly(8, 0), new TimeOnly(8, 0));

        Assert.False(schedule.IsActiveAt(new TimeOnly(8, 0)));
        Assert.False(schedule.IsActiveAt(new TimeOnly(14, 0)));
    }
}
