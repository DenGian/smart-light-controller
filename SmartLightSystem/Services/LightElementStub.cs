using SmartLightSystem.Interfaces;

namespace SmartLightSystem.Services;

public class LightElementStub : ILightElement
{
    private bool isEnabled = false;

    public bool IsEnabled
    {
        get { return isEnabled; }
    }

    public void Disable()
    {
        isEnabled = false;
    }

    public void Enable()
    {
        isEnabled = true;
    }
}
