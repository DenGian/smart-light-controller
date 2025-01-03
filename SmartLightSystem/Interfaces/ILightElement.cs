namespace SmartLightSystem.Interfaces;

public interface ILightElement
{
    public bool IsEnabled { get; }
    public void Enable();
    public void Disable();
}
