using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartLightSystem.Configuration;
using SmartLightSystem.Controllers;

namespace SmartLightSystem.HostedServices;

public sealed partial class LightControllerWorker(
    LightController controller,
    IOptions<SmartLightOptions> options,
    ILogger<LightControllerWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = options.Value.PollingInterval;
        LogControllerStarted(logger, pollingInterval);

        // Let host startup complete before the first external time read.
        await Task.Yield();
        await controller.RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(pollingInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await controller.RunOnceAsync(stoppingToken);
        }
    }

    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Information,
        Message = "Smart light controller started with a {PollingInterval} polling interval.")]
    private static partial void LogControllerStarted(ILogger logger, TimeSpan pollingInterval);
}
