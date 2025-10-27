using System.Text.Json.Serialization;

namespace McpServerAspNetCore.Models.OpenWeatherMap;

public class ForecastWeather
{
    [JsonPropertyName("city")]
    public required ForecastCity City { get; set; }

    [JsonPropertyName("cod")]
    public required string Code { get; set; }

    [JsonPropertyName("list")]
    public IEnumerable<ForecastWeatherData> WeatherData { get; set; } = [];
}