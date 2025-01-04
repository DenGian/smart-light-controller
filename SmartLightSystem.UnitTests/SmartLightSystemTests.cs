using Moq;
using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;

namespace SmartLightSystem.UnitTests;

public class LightControllerTests
{
    // Time configuration constants
    private const int EveningHour = 20; // 8 PM
    private const int MorningHour = 6; // 6 AM
    private const int MaxFailures = 2;

    // Fixed time values for controller configuration
    private static readonly TimeSpan StandardStartTime = new(EveningHour, 0, 0); // 8 PM
    private static readonly TimeSpan StandardEndTime = new(MorningHour, 0, 0); // 6 AM

    // Common time values used in tests
    private static readonly TimeSpan Midnight = new(0, 0, 0); // 12 AM
    private static readonly TimeSpan Noon = new(12, 0, 0); // 12 PM
    private static readonly TimeSpan OneMinute = new(0, 1, 0); // 1 minute interval

    // Test dependencies
    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly Mock<ILightElement> _lightElementMock;
    private readonly LightController _controller;

    public LightControllerTests()
    {
        _timeProviderMock = new Mock<ITimeProvider>();
        _lightElementMock = new Mock<ILightElement>();

        _controller = new LightController(_timeProviderMock.Object, _lightElementMock.Object)
        {
            StartTime = StandardStartTime,
            EndTime = StandardEndTime,
            MaxFailures = MaxFailures
        };
    }

    [Fact]
    public void Work_WhenTimeIsDuringActiveHours_EnablesLight()
    {
        // Arrange - Set time to 10 PM, which is during the active period (8 PM - 6 AM)
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(22, 0, 0)));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeIsOutsideActiveHours_DisablesLight()
    {
        // Arrange - Set time to noon, which is outside the active period
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(Noon));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeEqualsStartTime_EnablesLight()
    {
        // Arrange - Set time exactly to start time (8 PM)
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(StandardStartTime));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeEqualsEndTime_DisablesLight()
    {
        // Arrange - Set time exactly to end time (6 AM)
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(StandardEndTime));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeFailsAndNotInSafeMode_DoNothing()
    {
        // Arrange - Simulate a single time retrieval failure
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();

        // Act
        _controller.Work();

        // Assert - Should not enter safe mode on first failure
        Assert.False(_controller.InSafeMode);
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode()
    {
        // Arrange - Simulate repeated time retrieval failures
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();

        // Act - Trigger failures up to max limit
        for (int i = 0; i < MaxFailures; i++)
        {
            _controller.Work();
        }

        // Assert - Should enter safe mode and disable light
        Assert.True(_controller.InSafeMode);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenInSafeModeAndTimeSucceeds_ResetsSafeMode()
    {
        // Arrange
        // First trigger safe mode through repeated failures
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();
        for (int i = 0; i <= MaxFailures; i++)
        {
            _controller.Work();
        }

        // Then simulate successful time retrieval
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(Noon));

        // Act
        _controller.Work();

        // Assert - Should exit safe mode
        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void Work_WhenStartTimeEqualsEndTime_LightStaysDisabled()
    {
        // Arrange - Set start and end time to the same value
        var sameTime = Noon;
        _controller.StartTime = sameTime;
        _controller.EndTime = sameTime;
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(sameTime));

        // Act
        _controller.Work();

        // Assert - Light should stay disabled when start equals end time
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeSpansOvernight_EnablesLightAfterStartTime()
    {
        // Arrange - Test overnight period (10 PM - 6 AM), checking 11 PM
        _controller.StartTime = new TimeSpan(22, 0, 0);
        _controller.EndTime = StandardEndTime;
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(23, 0, 0)));

        // Act
        _controller.Work();

        // Assert - Light should be enabled during overnight period
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeSpansOvernight_EnablesLightBeforeEndTime()
    {
        // Arrange - Test overnight period (10 PM - 6 AM), checking 5 AM
        _controller.StartTime = new TimeSpan(22, 0, 0);
        _controller.EndTime = StandardEndTime;
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(5, 0, 0)));

        // Act
        _controller.Work();

        // Assert - Light should be enabled during overnight period
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_AfterFailure_ResetsFailureCountOnSuccess()
    {
        // Arrange
        // Step 1: Cause initial failure
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();
        _controller.Work();

        // Step 2: Succeed once
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(Noon));
        _controller.Work();

        // Step 3: Fail again - shouldn't trigger safe mode due to reset
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();
        _controller.Work();

        // Assert - Should not be in safe mode after single failure post-reset
        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void Work_WhenOneMinuteBeforeEndTime_StillEnabled()
    {
        // Arrange - Test one minute before end time
        var oneMinuteBeforeEnd = StandardEndTime.Subtract(OneMinute);
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(oneMinuteBeforeEnd));

        // Act
        _controller.Work();

        // Assert - Light should still be enabled just before end time
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeIsExactlyMidnight_HandlesOvernightPeriodCorrectly()
    {
        // Arrange - Set time to exactly midnight during overnight period
        _controller.StartTime = new TimeSpan(22, 0, 0); // 10 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(Midnight));

        // Act
        _controller.Work();

        // Assert - Light should be enabled at midnight during overnight period
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }
}
