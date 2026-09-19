namespace SmartLightController.Models;

public sealed record LightSchedule(TimeOnly Start, TimeOnly End)
{
    public bool IsActiveAt(TimeOnly time)
    {
        if (Start == End)
        {
            return false;
        }

        return Start < End
            ? time >= Start && time < End
            : time >= Start || time < End;
    }
}
