using SmartLightSystem.Interfaces;

namespace SmartLightSystem.Services;

public class TimeProvider : ITimeProvider
{
    public string Url
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public DateTime GetCurrentTime()
    {
        throw new NotImplementedException();
    }
}
