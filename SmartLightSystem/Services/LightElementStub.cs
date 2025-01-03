using SmartLightSystem.Interfaces;

namespace SmartLightSystem.Services;

public class LightElementStub : ILightElement
{
    private bool _isEnabled = false;

    public bool IsEnabled
    {
        get { return _isEnabled; }
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    public void Enable()
    {
        _isEnabled = true;
    }
}
