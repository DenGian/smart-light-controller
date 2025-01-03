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
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(22, 0, 0))); // 10 PM

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeIsOutsideActiveHours_DisablesLight()
    {
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(12, 0, 0))); // 12 PM

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeEqualsStartTime_EnablesLight()
    {
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(_startTime));

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Once);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeEqualsEndTime_DisablesLight()
    {
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(_endTime));

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeFailsAndNotInSafeMode_DoNothing()
    {
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();

        _controller.Work();

        Assert.False(_controller.InSafeMode);
        _lightElementMock.Verify(x => x.Enable(), Times.Never);
        _lightElementMock.Verify(x => x.Disable(), Times.Never);
    }

    [Fact]
    public void Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode()
    {
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Throws<Exception>();

        for (int i = 0; i < _maxFailures; i++)
        {
            _controller.Work();
        }

        Assert.True(_controller.InSafeMode);
        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenInSafeModeAndTimeSucceeds_ResetsSafeMode()
    {
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

        _controller.Work();

        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void Work_WhenStartTimeEqualsEndTime_LightStaysDisabled()
    {
        _controller.StartTime = new TimeSpan(12, 0, 0);
        _controller.EndTime = new TimeSpan(12, 0, 0);
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(12, 0, 0)));

        _controller.Work();

        _lightElementMock.Verify(x => x.Disable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeSpansOvernight_EnablesLightAfterStartTime()
    {
        _controller.StartTime = new TimeSpan(22, 0, 0); // 10 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(23, 0, 0))); // 11 PM

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_WhenTimeSpansOvernight_EnablesLightBeforeEndTime()
    {
        _controller.StartTime = new TimeSpan(22, 0, 0); // 10 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(5, 0, 0))); // 5 AM

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }

    [Fact]
    public void Work_AfterFailure_ResetsFailureCountOnSuccess()
    {
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

        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void Work_WhenOneMinuteBeforeEndTime_StillEnabled()
    {
        _controller.StartTime = new TimeSpan(20, 0, 0); // 8 PM
        _controller.EndTime = new TimeSpan(6, 0, 0);    // 6 AM
        _timeProviderMock.Setup(x => x.GetCurrentTime())
            .Returns(DateTime.Today.Add(new TimeSpan(5, 59, 0))); // 5:59 AM

        _controller.Work();

        _lightElementMock.Verify(x => x.Enable(), Times.Once);
    }
}
