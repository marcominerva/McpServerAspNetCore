using System.Text.Json.Serialization;

namespace McpServerAspNetCore.Models.OpenWeatherMap;

public class DailyForecastWeather
{
    [JsonPropertyName("city")]
    public required ForecastCity City { get; set; }

    [JsonPropertyName("cod")]
    public required string Code { get; set; }

    [JsonPropertyName("list")]
    public IEnumerable<DailyForecastWeatherData> WeatherData { get; set; } = [];
}
