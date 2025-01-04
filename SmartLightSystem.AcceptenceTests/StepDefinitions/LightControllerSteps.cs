using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;
using Xunit.Gherkin.Quick;

namespace SmartLightSystem.AcceptenceTests.StepDefinitions;

[FeatureFile("./Features/LightController.Feature")]
public sealed class LightControllerSteps : Feature
{
    // Setup constants
    private static readonly TimeSpan EveningStart = new TimeSpan(20, 0, 0); // 8 PM
    private static readonly TimeSpan MorningEnd = new TimeSpan(6, 0, 0);    // 6 AM
    private const int MaxFailures = 2;
    private const string UrlMockoon = "http://localhost:3000/api/time";

    private readonly ITimeProvider _timeProvider;
    private readonly ILightElement _lightElement;
    private readonly LightController _controller;

    public LightControllerSteps()
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

    [Given(@"the light is off")]
    public void SetLightOff()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=12:00:00"; // Noon
        _controller.Work();
        Assert.False(_lightElement.IsEnabled);
    }

    [Given(@"the light is on")]
    public void SetLightOn()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=22:00:00"; // 10 PM
        _controller.Work();
        Assert.True(_lightElement.IsEnabled);
    }

    [Given(@"the time is after start time")]
    public void SetTimeAfterStartTime()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=23:00:00"; // 11 PM
        _controller.Work();
    }

    [When(@"the time is outside active hours")]
    public void SetTimeOutsideActiveHours()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=14:00:00"; // 2 PM
    }

    [When(@"the time is within active hours")]
    public void SetTimeWithinActiveHours()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
    }

    [When(@"the time enters active hours")]
    public void TimeEntersActiveHours()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=20:00:00"; // 8 PM
    }

    [When(@"the time exits active hours")]
    public void TimeExitsActiveHours()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=06:00:00"; // 6 AM
    }

    [When(@"the time passes midnight")]
    public void TimePassesMidnight()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=00:00:00"; // Midnight
    }

    [When(@"the time equals start time")]
    public void TimeEqualsStartTime()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=20:00:00"; // 8 PM
    }

    [When(@"the time equals end time")]
    public void TimeEqualsEndTime()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=06:00:00"; // 6 AM
    }

    [Then(@"do nothing - light is off")]
    [Then(@"turn light off")]
    public void CheckLightOff()
    {
        _controller.Work();
        Assert.False(_lightElement.IsEnabled);
    }

    [Then(@"do nothing - light is on")]
    [Then(@"turn light on")]
    [Then(@"light stays on")]
    public void CheckLightOn()
    {
        _controller.Work();
        Assert.True(_lightElement.IsEnabled);
    }
}
