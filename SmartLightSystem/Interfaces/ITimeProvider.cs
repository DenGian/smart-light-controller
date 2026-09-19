namespace SmartLightSystem.Interfaces;

public interface ITimeProvider
{
    Task<DateTimeOffset> GetCurrentTimeAsync(CancellationToken cancellationToken);
}
