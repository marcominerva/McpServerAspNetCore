using System.Text.Json.Serialization;

namespace McpServerAspNetCore.Models.OpenWeatherMap;

public class Clouds
{
    [JsonPropertyName("all")]
    public int Cloudiness { get; set; }
}