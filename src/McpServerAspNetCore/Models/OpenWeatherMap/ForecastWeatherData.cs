using System.Text.Json.Serialization;
using McpServerAspNetCore.Models.OpenWeatherMap.Converters;

namespace McpServerAspNetCore.Models.OpenWeatherMap;

public class ForecastWeatherData
{
    [JsonPropertyName("dt")]
    [JsonConverter(typeof(UnixToDateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonConverter(typeof(UnixToDateTimeConverter))]
    public DateTime Sunrise { get; set; }

    [JsonConverter(typeof(UnixToDateTimeConverter))]
    public DateTime Sunset { get; set; }

    [JsonPropertyName("main")]
    public required ForecastWeatherDetail WeatherDetail { get; set; }

    [JsonPropertyName("weather")]
    public IEnumerable<WeatherCondition> Conditions { get; set; } = [];

    [JsonPropertyName("clouds")]
    public required Clouds Clouds { get; set; }

    [JsonPropertyName("wind")]
    public required Wind Wind { get; set; }
}