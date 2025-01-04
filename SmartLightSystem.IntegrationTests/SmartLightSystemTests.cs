using Xunit;
using System.Globalization;
using SmartLightSystem;
using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;

namespace SmartLightSystem.IntegrationTests;

public class LightControllerTests
{
    // Constants for URLs and configuration
    private const string UrlMockoon = "http://localhost:3000/api/time";
    private const string UrlMockoonException = "http://localhost:3000/api/time/exception";

    // Time constants for controller configuration
    private static readonly TimeSpan EveningStart = new(20, 0, 0); // 8 PM
    private static readonly TimeSpan MorningEnd = new(6, 0, 0); // 6 AM
    private const int MaxFailures = 2;

    // Time constants for tests
    private static readonly TimeSpan NightTime = new(22, 0, 0); // 10 PM
    private static readonly TimeSpan DayTime = new(12, 0, 0); // 12 PM
    private static readonly TimeSpan Midnight = new(0, 0, 0); // 12 AM
    private static readonly TimeSpan EveningTime = new(21, 0, 0); // 9 PM

    // Test dependencies
    private readonly ITimeProvider _timeProvider;
    private readonly ILightElement _lightElement;
    private readonly LightController _controller;

    public LightControllerTests()
    {
        _timeProvider = new TimeProviderReal();
        _lightElement = new LightElementStub();

        _controller = new LightController(_timeProvider, _lightElement)
        {
            StartTime = EveningStart,
            EndTime = MorningEnd,
            MaxFailures = MaxFailures
        };
    }

    [Fact]
    public void WhenLightEnabledAndTimeWithinBoundaries_StaysEnabled()
    {
        // Arrange - First enable light during evening hours
        _timeProvider.Url = $"{UrlMockoon}?time={EveningTime:hh\\:mm\\:ss}";
        _controller.Work();
        Assert.True(_lightElement.IsEnabled, "Light should be enabled at 9 PM");

        // Act - Check another time within active period
        _timeProvider.Url = $"{UrlMockoon}?time={NightTime:hh\\:mm\\:ss}";
        _controller.Work();

        // Assert
        Assert.True(_lightElement.IsEnabled, "Light should remain enabled at 10 PM");
    }

    [Fact]
    public void WhenLightDisabledAndTimeOutsideBoundaries_StaysDisabled()
    {
        // Arrange - First disable light during day hours
        _timeProvider.Url = $"{UrlMockoon}?time={DayTime:hh\\:mm\\:ss}";
        _controller.Work();
        Assert.False(_lightElement.IsEnabled, "Light should be disabled at noon");

        // Act - Check another time outside active period
        _timeProvider.Url = $"{UrlMockoon}?time=14:00:00"; // 2 PM
        _controller.Work();

        // Assert
        Assert.False(_lightElement.IsEnabled, "Light should remain disabled at 2 PM");
    }

    [Fact]
    public void WhenTimeWithinActiveHours_EnablesLight()
    {
        // Arrange - Set time during active period
        _timeProvider.Url = $"{UrlMockoon}?time={NightTime:hh\\:mm\\:ss}";

        // Act
        _controller.Work();

        // Assert
        Assert.True(_lightElement.IsEnabled, "Light should be enabled during active hours");
    }

    [Fact]
    public void WhenTimeOutsideActiveHours_DisablesLight()
    {
        // Arrange - Set time outside active period
        _timeProvider.Url = $"{UrlMockoon}?time={DayTime:hh\\:mm\\:ss}";

        // Act
        _controller.Work();

        // Assert
        Assert.False(_lightElement.IsEnabled, "Light should be disabled outside active hours");
    }

    [Fact]
    public void WhenTimeFailsAndNotInSafeMode_MaintainsState()
    {
        // Arrange - First set a valid state during active hours
        _timeProvider.Url = $"{UrlMockoon}?time={EveningTime:hh\\:mm\\:ss}";
        _controller.Work();
        Assert.True(_lightElement.IsEnabled, "Light should initially be enabled");

        // Act - Simulate API failure
        _timeProvider.Url = UrlMockoonException;
        _controller.Work();

        // Assert - State should be maintained with single failure
        Assert.True(_lightElement.IsEnabled, "Light should maintain state after single failure");
        Assert.False(_controller.InSafeMode, "Should not enter safe mode after single failure");
    }

    [Fact]
    public void WhenTimeFailsAndMaxFailuresReached_EntersSafeMode()
    {
        // Arrange - First set a valid state during active hours
        _timeProvider.Url = $"{UrlMockoon}?time={EveningTime:hh\\:mm\\:ss}";
        _controller.Work();
        Assert.True(_lightElement.IsEnabled, "Light should initially be enabled");

        // Act - Simulate repeated API failures
        _timeProvider.Url = UrlMockoonException;
        for (var i = 0; i < _controller.MaxFailures; i++) _controller.Work();

        // Assert
        Assert.True(_controller.InSafeMode, "Should enter safe mode after max failures");
        Assert.False(_lightElement.IsEnabled, "Light should be disabled in safe mode");
    }

    [Fact]
    public void WhenInSafeModeAndTimeSucceeds_ResetsAndReturnsToNormal()
    {
        // Arrange - First trigger safe mode through repeated failures
        _timeProvider.Url = UrlMockoonException;
        for (var i = 0; i < _controller.MaxFailures; i++) _controller.Work();
        Assert.True(_controller.InSafeMode, "Should be in safe mode after failures");

        // Act - Simulate API recovery
        _timeProvider.Url = $"{UrlMockoon}?time={DayTime:hh\\:mm\\:ss}";
        _controller.Work();

        // Assert
        Assert.False(_controller.InSafeMode, "Should exit safe mode after successful API call");
    }

    [Fact]
    public void WhenTimeIsExactlyStartTime_EnablesLight()
    {
        // Arrange - Set time exactly to start time
        _timeProvider.Url = $"{UrlMockoon}?time={EveningStart:hh\\:mm\\:ss}";

        // Act
        _controller.Work();

        // Assert
        Assert.True(_lightElement.IsEnabled, "Light should be enabled exactly at start time");
    }

    [Fact]
    public void WhenTimeIsExactlyEndTime_DisablesLight()
    {
        // Arrange - Set time exactly to end time
        _timeProvider.Url = $"{UrlMockoon}?time={MorningEnd:hh\\:mm\\:ss}";

        // Act
        _controller.Work();

        // Assert
        Assert.False(_lightElement.IsEnabled, "Light should be disabled exactly at end time");
    }

    [Fact]
    public void WhenTimeSpansOvernight_HandlesCorrectly()
    {
        // Arrange - Test with midnight time
        _timeProvider.Url = $"{UrlMockoon}?time={Midnight:hh\\:mm\\:ss}";

        // Act
        _controller.Work();

        // Assert
        Assert.True(_lightElement.IsEnabled, "Light should be enabled at midnight during overnight period");
    }

    [Fact]
    public void WhenAPIReturnsInvalidTimeFormat_EntersSafeMode()
    {
        // Arrange - Set invalid time format
        _timeProvider.Url = $"{UrlMockoon}?time=invalid";

        // Act - Try to work with invalid time
        for (var i = 0; i < MaxFailures; i++) _controller.Work();

        // Assert
        Assert.True(_controller.InSafeMode, "Controller should enter safe mode with invalid time format");
        Assert.False(_lightElement.IsEnabled, "Light should be disabled in safe mode");
    }

    [Fact]
    public void WhenFailureOccursAndRecovers_ResetsFailureCount()
    {
        // Arrange
        // First cause a failure
        _timeProvider.Url = UrlMockoonException;
        _controller.Work();

        // Then succeed
        _timeProvider.Url = $"{UrlMockoon}?time={DayTime:hh\\:mm\\:ss}";
        _controller.Work();

        // Then fail again once
        _timeProvider.Url = UrlMockoonException;
        _controller.Work();

        // Assert - Should not be in safe mode after single failure post-recovery
        Assert.False(_controller.InSafeMode, "Should not enter safe mode after single failure post-recovery");
    }
}
