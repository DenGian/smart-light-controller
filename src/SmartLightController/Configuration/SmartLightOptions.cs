namespace SmartLightController.Configuration;

public sealed class SmartLightOptions
{
    public const string SectionName = "SmartLight";

    public TimeOnly ActiveStart { get; init; } = new(20, 0);

    public TimeOnly ActiveEnd { get; init; } = new(6, 0);

    public int MaxConsecutiveFailures { get; init; } = 3;

    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);

    public string TimeZoneId { get; init; } = "Europe/Brussels";

    public Uri TimeServiceBaseUri { get; init; } = new("https://worldtimeapi.org/api/timezone/");

    public TimeSpan HttpTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
