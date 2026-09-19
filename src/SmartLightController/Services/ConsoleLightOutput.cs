using SmartLightController.Interfaces;

namespace SmartLightController.Services;

public sealed class ConsoleLightOutput : ILightOutput
{
    public bool IsEnabled { get; private set; }

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }
}
