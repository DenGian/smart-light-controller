using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartLightController.Configuration;
using SmartLightController.Controllers;
using SmartLightController.Exceptions;
using SmartLightController.Interfaces;

namespace SmartLightController.UnitTests;

public sealed class LightControllerTests
{
    [Fact]
    public async Task SuccessfulRead_AppliesScheduledState()
    {
        var (controller, light, _) = CreateController(new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero));

        await controller.RunOnceAsync();

        Assert.True(light.IsEnabled);
        Assert.Equal(1, light.StateChanges);
        Assert.Equal(0, controller.ConsecutiveFailures);
    }

    [Fact]
    public async Task RepeatedScheduledState_DoesNotRepeatSideEffect()
    {
        var (controller, light, _) = CreateController(new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero));

        await controller.RunOnceAsync();
        await controller.RunOnceAsync();

        Assert.Equal(1, light.StateChanges);
    }

    [Fact]
    public async Task FailureBelowThreshold_PreservesCurrentState()
    {
        var (controller, light, time) = CreateController(new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero));
        await controller.RunOnceAsync();
        time.Exception = new TimeProviderException("Unavailable");

        await controller.RunOnceAsync();

        Assert.True(light.IsEnabled);
        Assert.False(controller.IsInSafeMode);
        Assert.Equal(1, controller.ConsecutiveFailures);
    }

    [Fact]
    public async Task FailureAtThreshold_EntersSafeModeAndTurnsLightOff()
    {
        var (controller, light, time) = CreateController(new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero));
        await controller.RunOnceAsync();
        time.Exception = new TimeProviderException("Unavailable");

        await controller.RunOnceAsync();
        await controller.RunOnceAsync();

        Assert.False(light.IsEnabled);
        Assert.True(controller.IsInSafeMode);
        Assert.Equal(2, controller.ConsecutiveFailures);
    }

    [Fact]
    public async Task SafeModeTransition_IsLoggedExactlyOnce()
    {
        var logger = new RecordingLogger();
        var (controller, _, time) = CreateController(
            new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero),
            logger);
        time.Exception = new TimeProviderException("Unavailable");

        await controller.RunOnceAsync();
        await controller.RunOnceAsync();
        await controller.RunOnceAsync();

        var transition = Assert.Single(logger.Entries, entry => entry.EventId.Id == 3);
        Assert.Equal(LogLevel.Error, transition.Level);
    }

    [Fact]
    public async Task FailureWhileAlreadyInSafeMode_RemainsSafeWithoutRepeatingOutput()
    {
        var (controller, light, time) = CreateController(new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero));
        await controller.RunOnceAsync();
        time.Exception = new TimeProviderException("Unavailable");
        await controller.RunOnceAsync();
        await controller.RunOnceAsync();

        await controller.RunOnceAsync();

        Assert.True(controller.IsInSafeMode);
        Assert.Equal(3, controller.ConsecutiveFailures);
        Assert.False(light.IsEnabled);
        Assert.Equal(2, light.StateChanges);
    }

    [Fact]
    public async Task SuccessAfterSafeMode_RecoversAndResetsFailures()
    {
        var logger = new RecordingLogger();
        var (controller, light, time) = CreateController(
            new DateTimeOffset(2026, 1, 1, 21, 0, 0, TimeSpan.Zero),
            logger);
        time.Exception = new TimeProviderException("Unavailable");
        await controller.RunOnceAsync();
        await controller.RunOnceAsync();
        time.Exception = null;

        await controller.RunOnceAsync();

        Assert.True(light.IsEnabled);
        Assert.False(controller.IsInSafeMode);
        Assert.Equal(0, controller.ConsecutiveFailures);
        Assert.Single(logger.Entries, entry => entry.EventId.Id == 1);
    }

    [Fact]
    public async Task SuccessBetweenFailures_ResetsConsecutiveCount()
    {
        var (controller, _, time) = CreateController(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        time.Exception = new TimeProviderException("Unavailable");
        await controller.RunOnceAsync();
        time.Exception = null;
        await controller.RunOnceAsync();
        time.Exception = new TimeProviderException("Unavailable again");

        await controller.RunOnceAsync();

        Assert.Equal(1, controller.ConsecutiveFailures);
        Assert.False(controller.IsInSafeMode);
    }

    [Fact]
    public async Task Cancellation_IsNotTreatedAsAProviderFailure()
    {
        var provider = new CancellingTimeProvider();
        var controller = new LightController(
            provider,
            new RecordingLightOutput(),
            Options.Create(CreateOptions()),
            NullLogger<LightController>.Instance);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => controller.RunOnceAsync(source.Token));
        Assert.Equal(0, controller.ConsecutiveFailures);
    }

    private static (LightController Controller, RecordingLightOutput Light, MutableTimeProvider Time) CreateController(
        DateTimeOffset currentTime,
        ILogger<LightController>? logger = null)
    {
        var time = new MutableTimeProvider { CurrentTime = currentTime };
        var light = new RecordingLightOutput();
        var controller = new LightController(
            time,
            light,
            Options.Create(CreateOptions()),
            logger ?? NullLogger<LightController>.Instance);
        return (controller, light, time);
    }

    private static SmartLightOptions CreateOptions() => new()
    {
        ActiveStart = new TimeOnly(20, 0),
        ActiveEnd = new TimeOnly(6, 0),
        MaxConsecutiveFailures = 2,
    };

    private sealed class MutableTimeProvider : ITimeProvider
    {
        public DateTimeOffset CurrentTime { get; init; }

        public Exception? Exception { get; set; }

        public Task<DateTimeOffset> GetCurrentTimeAsync(CancellationToken cancellationToken) =>
            Exception is null
                ? Task.FromResult(CurrentTime)
                : Task.FromException<DateTimeOffset>(Exception);
    }

    private sealed class CancellingTimeProvider : ITimeProvider
    {
        public Task<DateTimeOffset> GetCurrentTimeAsync(CancellationToken cancellationToken) =>
            Task.FromCanceled<DateTimeOffset>(cancellationToken);
    }

    private sealed class RecordingLightOutput : ILightOutput
    {
        public bool IsEnabled { get; private set; }

        public int StateChanges { get; private set; }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            StateChanges++;
        }
    }

    private sealed class RecordingLogger : ILogger<LightController>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add(new LogEntry(logLevel, eventId));
    }

    private sealed record LogEntry(LogLevel Level, EventId EventId);
}
