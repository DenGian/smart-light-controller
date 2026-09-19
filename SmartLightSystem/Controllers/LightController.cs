using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartLightSystem.Configuration;
using SmartLightSystem.Exceptions;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Models;

namespace SmartLightSystem.Controllers;

public sealed partial class LightController
{
    private readonly ITimeProvider _timeProvider;
    private readonly ILightOutput _lightOutput;
    private readonly LightSchedule _schedule;
    private readonly SmartLightOptions _options;
    private readonly ILogger<LightController> _logger;

    public LightController(
        ITimeProvider timeProvider,
        ILightOutput lightOutput,
        IOptions<SmartLightOptions> options,
        ILogger<LightController> logger)
    {
        _timeProvider = timeProvider;
        _lightOutput = lightOutput;
        _options = options.Value;
        _schedule = new LightSchedule(_options.ActiveStart, _options.ActiveEnd);
        _logger = logger;
    }

    public int ConsecutiveFailures { get; private set; }

    public bool IsInSafeMode => ConsecutiveFailures >= _options.MaxConsecutiveFailures;

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentTime = await _timeProvider.GetCurrentTimeAsync(cancellationToken);
            var wasInSafeMode = IsInSafeMode;
            ConsecutiveFailures = 0;

            if (wasInSafeMode)
            {
                LogTimeSourceRecovered(_logger);
            }

            ApplyState(_schedule.IsActiveAt(TimeOnly.FromDateTime(currentTime.DateTime)));
        }
        catch (TimeProviderException exception)
        {
            ConsecutiveFailures++;
            LogTimeReadFailed(
                _logger,
                exception,
                ConsecutiveFailures,
                _options.MaxConsecutiveFailures);

            if (IsInSafeMode)
            {
                ApplyState(false);
                LogSafeModeActive(_logger, ConsecutiveFailures);
            }
        }
    }

    private void ApplyState(bool shouldBeEnabled)
    {
        if (_lightOutput.IsEnabled == shouldBeEnabled)
        {
            return;
        }

        _lightOutput.SetEnabled(shouldBeEnabled);
        LogLightStateChanged(_logger, shouldBeEnabled);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Time source recovered; leaving safe mode.")]
    private static partial void LogTimeSourceRecovered(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Time read failed ({FailureCount}/{FailureThreshold}).")]
    private static partial void LogTimeReadFailed(
        ILogger logger,
        Exception exception,
        int failureCount,
        int failureThreshold);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Safe mode active after {FailureCount} consecutive time-read failures; light is off.")]
    private static partial void LogSafeModeActive(ILogger logger, int failureCount);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Light output changed: {Enabled}.")]
    private static partial void LogLightStateChanged(ILogger logger, bool enabled);
}
