namespace SmartLightController.Interfaces;

public interface ILightOutput
{
    bool IsEnabled { get; }

    void SetEnabled(bool enabled);
}
