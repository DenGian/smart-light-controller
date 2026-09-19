using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartLightController.Configuration;
using SmartLightController.Controllers;
using SmartLightController.Exceptions;
using SmartLightController.Interfaces;

namespace SmartLightController.AcceptanceTests;

public sealed class LightControllerAcceptanceTests
{
    [Fact]
    public async Task LightFollowsAnOvernightScheduleAcrossBoundaries()
    {
        var context = CreateContext();

        context.Time.CurrentTime = At(19, 59);
        await context.Controller.RunOnceAsync();
        Assert.False(context.Light.IsEnabled);

        context.Time.CurrentTime = At(20, 0);
        await context.Controller.RunOnceAsync();
        Assert.True(context.Light.IsEnabled);

        context.Time.CurrentTime = At(0, 0);
        await context.Controller.RunOnceAsync();
        Assert.True(context.Light.IsEnabled);

        context.Time.CurrentTime = At(6, 0);
        await context.Controller.RunOnceAsync();
        Assert.False(context.Light.IsEnabled);
    }

    [Fact]
    public async Task OneFailurePreservesAnEnabledLight()
    {
        var context = CreateContext();
        context.Time.CurrentTime = At(21, 0);
        await context.Controller.RunOnceAsync();
        context.Time.ShouldFail = true;

        await context.Controller.RunOnceAsync();

        Assert.True(context.Light.IsEnabled);
        Assert.False(context.Controller.IsInSafeMode);
    }

    [Fact]
    public async Task ReachingFailureThresholdTurnsLightOff()
    {
        var context = CreateContext();
        context.Time.CurrentTime = At(21, 0);
        await context.Controller.RunOnceAsync();
        context.Time.ShouldFail = true;

        await context.Controller.RunOnceAsync();
        await context.Controller.RunOnceAsync();

        Assert.False(context.Light.IsEnabled);
        Assert.True(context.Controller.IsInSafeMode);
    }

    [Fact]
    public async Task RecoveredTimeSourceRestoresScheduledOperation()
    {
        var context = CreateContext();
        context.Time.ShouldFail = true;
        await context.Controller.RunOnceAsync();
        await context.Controller.RunOnceAsync();
        context.Time.ShouldFail = false;
        context.Time.CurrentTime = At(21, 0);

        await context.Controller.RunOnceAsync();

        Assert.True(context.Light.IsEnabled);
        Assert.False(context.Controller.IsInSafeMode);
    }

    private static AcceptanceContext CreateContext()
    {
        var options = Options.Create(new SmartLightOptions
        {
            ActiveStart = new TimeOnly(20, 0),
            ActiveEnd = new TimeOnly(6, 0),
            MaxConsecutiveFailures = 2,
        });
        var time = new ScenarioTimeProvider();
        var light = new ScenarioLightOutput();
        var controller = new LightController(time, light, options, NullLogger<LightController>.Instance);
        return new AcceptanceContext(controller, time, light);
    }

    private static DateTimeOffset At(int hour, int minute) =>
        new(2026, 1, 1, hour, minute, 0, TimeSpan.FromHours(1));

    private sealed record AcceptanceContext(
        LightController Controller,
        ScenarioTimeProvider Time,
        ScenarioLightOutput Light);

    private sealed class ScenarioTimeProvider : ITimeProvider
    {
        public DateTimeOffset CurrentTime { get; set; } = At(12, 0);

        public bool ShouldFail { get; set; }

        public Task<DateTimeOffset> GetCurrentTimeAsync(CancellationToken cancellationToken) =>
            ShouldFail
                ? Task.FromException<DateTimeOffset>(new TimeProviderException("Scenario failure"))
                : Task.FromResult(CurrentTime);
    }

    private sealed class ScenarioLightOutput : ILightOutput
    {
        public bool IsEnabled { get; private set; }

        public void SetEnabled(bool enabled) => IsEnabled = enabled;
    }
}
