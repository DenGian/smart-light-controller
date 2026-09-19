using System.Text.Json.Serialization;

namespace SmartLightSystem.Models;

internal sealed record TimeResponse
{
    [JsonPropertyName("datetime")]
    public DateTimeOffset? DateTime { get; init; }
}
