using SmartLightSystem.Interfaces;
using SmartLightSystem.Models;
using Newtonsoft.Json;

namespace SmartLightSystem.Services;

public class TimeProviderReal : ITimeProvider
{
    private string _url = "http://worldtimeapi.org/api/timezone/Europe/Brussels";

    public string Url
    {
        get => _url;
        set => _url = value;
    }

    public DateTime GetCurrentTime()
    {
        using (var httpClient = new HttpClient())
        {
            var httpResponse = httpClient.GetAsync(_url).GetAwaiter().GetResult();
            var response = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var timeResponse = JsonConvert.DeserializeObject<TimeResponse>(response);
            if (timeResponse != null)
            {
                return DateTime.Parse(timeResponse.datetime);
            }
        }
        throw new Exception("Failed to get the current time.");
    }
}
