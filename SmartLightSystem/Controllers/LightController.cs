using SmartLightSystem.Interfaces;

namespace SmartLightSystem.Controllers;

public class LightController
{
    private readonly ITimeProvider timeProvider;
    private readonly ILightElement lightElement;

    private int failures = 0;

    private TimeSpan startTime;
    public TimeSpan StartTime
    {
        get { return startTime; }
        set { startTime = value; }
    }

    private TimeSpan endTime;
    public TimeSpan EndTime
    {
        get { return endTime; }
        set { endTime = value; }
    }

    private int maxFailures;
    public int MaxFailures
    {
        get { return maxFailures; }
        set { maxFailures = value; }
    }

    public bool InSafeMode
    {
        get { return (failures < MaxFailures) ? false : true; }
    }

    public LightController(ITimeProvider timeProvider, ILightElement lightElement)
    {
        this.timeProvider = timeProvider;
        this.lightElement = lightElement;
    }

    public void Work()
    {
        try
        {
            DateTime currentTime = timeProvider.GetCurrentTime();
            failures = 0;

            TimeSpan current = currentTime.TimeOfDay;

            if (StartTime <= EndTime)
            {
                if (current >= StartTime && current < EndTime)
                {
                    lightElement.Enable();
                }
                else
                {
                    lightElement.Disable();
                }
            }
            else
            {
                if (current >= StartTime || current < EndTime)
                {
                    lightElement.Enable();
                }
                else
                {
                    lightElement.Disable();
                }
            }
        }
        catch
        {
            failures++;
            if (failures >= MaxFailures)
            {
                lightElement.Disable();
            }
        }
    }
}
