using Xunit;
using System.Globalization;
using SmartLightSystem;
using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;

namespace SmartLightSystem.IntegrationTests;

public class LightControllerTests
{
    private const string UrlMockoon = "http://localhost:3000/api/time";
    private const string UrlMockoonException = "http://localhost:3000/api/time/exception";
    private readonly TimeSpan _startTime = new TimeSpan(20, 0, 0); // 8 PM
    private readonly TimeSpan _endTime = new TimeSpan(6, 0, 0);   // 6 AM
    private readonly int _maxFailures = 2;

    private readonly ITimeProvider _timeProvider;
    private readonly ILightElement _lightElement;
    private readonly LightController _controller;

    public LightControllerTests()
    {
        _timeProvider = new TimeProviderReal();
        _lightElement = new LightElementStub();

        _controller = new LightController(_timeProvider, _lightElement)
        {
            StartTime = _startTime,
            EndTime = _endTime,
            MaxFailures = _maxFailures
        };
    }

    [Fact]
    public void WhenLightEnabledAndTimeWithinBoundaries_StaysEnabled()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
        _controller.Work();
        Assert.True(_lightElement.IsEnabled);

        _timeProvider.Url = $"{UrlMockoon}?time=22:00:00"; // 10 PM
        _controller.Work();

        Assert.True(_lightElement.IsEnabled);
    }

    [Fact]
    public void WhenLightDisabledAndTimeOutsideBoundaries_StaysDisabled()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=12:00:00"; // 12 PM
        _controller.Work();
        Assert.False(_lightElement.IsEnabled);

        _timeProvider.Url = $"{UrlMockoon}?time=14:00:00"; // 2 PM
        _controller.Work();

        Assert.False(_lightElement.IsEnabled);
    }

    [Fact]
    public void WhenTimeWithinActiveHours_EnablesLight()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=22:00:00"; // 10 PM

        _controller.Work();

        Assert.True(_lightElement.IsEnabled);
    }

    [Fact]
    public void WhenTimeOutsideActiveHours_DisablesLight()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=12:00:00"; // 12 PM

        _controller.Work();

        Assert.False(_lightElement.IsEnabled);
    }

    [Fact]
    public void WhenTimeFailsAndNotInSafeMode_MaintainsState()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
        _controller.Work();
        Assert.True(_lightElement.IsEnabled);

        _timeProvider.Url = UrlMockoonException;
        _controller.Work();

        Assert.True(_lightElement.IsEnabled);
        Assert.False(_controller.InSafeMode);
    }

    [Fact]
    public void WhenTimeFailsAndMaxFailuresReached_EntersSafeMode()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
        _controller.Work();
        Assert.True(_lightElement.IsEnabled);

        _timeProvider.Url = UrlMockoonException;
        for (int i = 0; i < _controller.MaxFailures; i++)
        {
            _controller.Work();
        }

        Assert.True(_controller.InSafeMode);
        Assert.False(_lightElement.IsEnabled);
    }

    [Fact]
    public void WhenInSafeModeAndTimeSucceeds_ResetsAndReturnsToNormal()
    {
        _timeProvider.Url = UrlMockoonException;
        for (int i = 0; i < _controller.MaxFailures; i++)
        {
            _controller.Work();
        }
        Assert.True(_controller.InSafeMode);

        _timeProvider.Url = $"{UrlMockoon}?time=12:00:00"; // 12 PM
        _controller.Work();

        Assert.False(_controller.InSafeMode);
    }
}
