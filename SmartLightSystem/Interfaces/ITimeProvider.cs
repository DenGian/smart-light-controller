namespace SmartLightSystem.Interfaces;

public interface ITimeProvider
{
    string Url { get; set; }
    DateTime GetCurrentTime();
}
