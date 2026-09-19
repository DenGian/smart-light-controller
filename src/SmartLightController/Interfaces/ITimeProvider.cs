namespace SmartLightController.Interfaces;

public interface ITimeProvider
{
    Task<DateTimeOffset> GetCurrentTimeAsync(CancellationToken cancellationToken);
}
