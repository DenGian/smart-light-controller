using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;
using Xunit.Gherkin.Quick;

namespace SmartLightSystem.AcceptenceTests.StepDefinitions;

[FeatureFile("./Features/LightControllerFailure.Feature")]
public sealed class LightControllerFailureSteps : Feature
{
    // Setup constants
    private static readonly TimeSpan EveningStart = new TimeSpan(20, 0, 0);
    private static readonly TimeSpan MorningEnd = new TimeSpan(6, 0, 0);
    private const int MaxFailures = 2;
    private const string UrlMockoon = "http://localhost:3000/api/time";
    private const string UrlMockoonException = "http://localhost:3000/api/time/exception";

    private readonly ITimeProvider _timeProvider;
    private readonly ILightElement _lightElement;
    private readonly LightController _controller;

    public LightControllerFailureSteps()
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

    [Given(@"the light is on")]
    public void SetLightOn()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
        _controller.Work();
        Assert.True(_lightElement.IsEnabled);
    }

    [Given(@"controller in safe mode")]
    public void SetControllerInSafeMode()
    {
        _timeProvider.Url = UrlMockoonException;
        for (int i = 0; i < MaxFailures; i++)
        {
            _controller.Work();
        }
        Assert.True(_controller.InSafeMode);
    }

    [And(@"number of failures is less than maximum")]
    public void ResetFailureCount()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
        _controller.Work();
    }

    [And(@"number of failures is maximum failures minus one")]
    public void SetFailurestoMaxMinusOne()
    {
        _timeProvider.Url = UrlMockoonException;
        for (int i = 1; i < MaxFailures; i++)
        {
            _controller.Work();
        }
    }

    [When(@"getting the time fails")]
    public void TriggerTimeServiceFailure()
    {
        _timeProvider.Url = UrlMockoonException;
        _controller.Work();
    }

    [When(@"the time service recovers")]
    public void TimeServiceRecovers()
    {
        _timeProvider.Url = $"{UrlMockoon}?time=21:00:00"; // 9 PM
    }

    [Then(@"do nothing - light stays on")]
    public void VerifyLightStaysOn()
    {
        Assert.True(_lightElement.IsEnabled);
    }

    [Then(@"turn light off")]
    public void VerifyLightOff()
    {
        Assert.False(_lightElement.IsEnabled);
    }

    [And(@"set controller in safe mode")]
    public void VerifyInSafeMode()
    {
        Assert.True(_controller.InSafeMode);
    }

    [And(@"set controller in normal mode")]
    public void VerifyInNormalMode()
    {
        _controller.Work();
        Assert.False(_controller.InSafeMode);
    }

    [And(@"light state updates according to time")]
    public void VerifyLightStateUpdates()
    {
        _controller.Work();
        // Since we recovered at 9 PM, light should be on
        Assert.True(_lightElement.IsEnabled);
    }
}
