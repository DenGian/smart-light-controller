using SmartLightSystem.Interfaces;

namespace SmartLightSystem.Controllers;

public class LightController
{
    private readonly ITimeProvider _timeProvider;
    private readonly ILightElement _lightElement;

    private int _failures = 0;

    private TimeSpan _startTime;
    public TimeSpan StartTime
    {
        get { return _startTime; }
        set { _startTime = value; }
    }

    private TimeSpan _endTime;
    public TimeSpan EndTime
    {
        get { return _endTime; }
        set { _endTime = value; }
    }

    private int _maxFailures;
    public int MaxFailures
    {
        get { return _maxFailures; }
        set { _maxFailures = value; }
    }

    public bool InSafeMode
    {
        get { return _failures >= MaxFailures; }
    }

    public LightController(ITimeProvider timeProvider, ILightElement lightElement)
    {
        _timeProvider = timeProvider;
        _lightElement = lightElement;
    }

    public void Work()
    {
        try
        {
            DateTime currentTime = _timeProvider.GetCurrentTime();
            _failures = 0;

            TimeSpan current = currentTime.TimeOfDay;

            if (StartTime <= EndTime)
            {
                if (current >= StartTime && current < EndTime)
                {
                    _lightElement.Enable();
                }
                else
                {
                    _lightElement.Disable();
                }
            }
            else
            {
                if (current >= StartTime || current < EndTime)
                {
                    _lightElement.Enable();
                }
                else
                {
                    _lightElement.Disable();
                }
            }
        }
        catch
        {
            _failures++;
            if (_failures >= MaxFailures)
            {
                _lightElement.Disable();
            }
        }
    }
}
