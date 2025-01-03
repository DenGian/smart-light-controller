using Moq;
using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;

namespace SmartLightSystem.UnitTests;

public class LightControllerTests
{
    private readonly TimeSpan _startTime = new TimeSpan(20, 0, 0); // 8 PM
    private readonly TimeSpan _endTime = new TimeSpan(6, 0, 0);   // 6 AM
    private readonly int _maxFailures = 2;

    private readonly Mock<ITimeProvider> _timeProviderMock;
    private readonly Mock<ILightElement> _lightElementMock;
    private readonly LightController _controller;

    public LightControllerTests()
    {
        _timeProviderMock = new Mock<ITimeProvider>();
        _lightElementMock = new Mock<ILightElement>();

        _controller = new LightController(_timeProviderMock.Object, _lightElementMock.Object)
        {
            StartTime = _startTime,
            EndTime = _endTime,
            MaxFailures = _maxFailures
        };
    }

    [Fact]
    public void Work_WhenTimeIsDuringActiveHours_EnablesLight()
    {
        // Arrange
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(22, 0, 0))); // 10 PM

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeIsOutsideActiveHours_DisablesLight()
    {
        // Arrange
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(12, 0, 0))); // 12 PM

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeEqualsStartTime_EnablesLight()
    {
        // Arrange
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(_startTime));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeEqualsEndTime_DisablesLight()
    {
        // Arrange
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(_endTime));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeFailsAndNotInSafeMode_DoNothing()
    {
        // Arrange
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();

        // Act
        _controller.Work();

        // Assert
        Assert.False(_controller.InSafeMode);
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode()
    {
        // Arrange
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();

        // Act
        for (int i = 0; i < _maxFailures; i++)
        {
            _controller.Work();
        }

        // Assert
        Assert.True(_controller.InSafeMode);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenInSafeModeAndTimeSucceeds_ResetsSafeMode()
    {
        // Arrange
        // First trigger safe mode
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();
        for (int i = 0; i <= _maxFailures; i++)
        {
            _controller.Work();
        }

        // Then succeed
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(12, 0, 0)));

        // Act
        _controller.Work();

        // Assert
        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void Work_WhenStartTimeEqualsEndTime_LightStaysDisabled()
    {
        // Arrange
        _controller.StartTime = new TimeSpan(12, 0, 0);
        _controller.EndTime = new TimeSpan(12, 0, 0);
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(12, 0, 0)));

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeSpansOvernight_EnablesLightAfterStartTime()
    {
        // Arrange
        _controller.StartTime = new TimeSpan(22, 0, 0); // 10 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(23, 0, 0))); // 11 PM

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeSpansOvernight_EnablesLightBeforeEndTime()
    {
        // Arrange
        _controller.StartTime = new TimeSpan(22, 0, 0); // 10 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(5, 0, 0))); // 5 AM

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_AfterFailure_ResetsFailureCountOnSuccess()
    {
        // Arrange
        // First cause a failure
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();
        _controller.Work();

        // Then succeed
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(12, 0, 0)));
        _controller.Work();

        // Then fail again - shouldn't enter safe mode yet
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();
        _controller.Work();

        // Assert
        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void Work_WhenOneMinuteBeforeEndTime_StillEnabled()
    {
        // Arrange
        _controller.StartTime = new TimeSpan(20, 0, 0); // 8 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(5, 59, 0))); // 5:59 AM

        // Act
        _controller.Work();

        // Assert
        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }
}
