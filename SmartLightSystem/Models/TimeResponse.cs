namespace SmartLightSystem.Models;

public class TimeResponse
{
    public string datetime { get; set; }
    public int day_of_week { get; set; }
    public int day_of_year { get; set; }
    public int week_number { get; set; }
    public string timezone { get; set; }
}
