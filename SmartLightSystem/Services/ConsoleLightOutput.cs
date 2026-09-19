using Microsoft.Extensions.Logging;
using SmartLightSystem.Interfaces;

namespace SmartLightSystem.Services;

public sealed partial class ConsoleLightOutput(ILogger<ConsoleLightOutput> logger) : ILightOutput
{
    public bool IsEnabled { get; private set; }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        LogStateChanged(logger, enabled);
    }

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Demo light output changed: {Enabled}")]
    private static partial void LogStateChanged(ILogger logger, bool enabled);
}
