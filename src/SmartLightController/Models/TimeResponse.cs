using System.Text.Json.Serialization;

namespace SmartLightController.Models;

internal sealed record TimeResponse
{
    [JsonPropertyName("datetime")]
    public DateTimeOffset? DateTime { get; init; }
}
