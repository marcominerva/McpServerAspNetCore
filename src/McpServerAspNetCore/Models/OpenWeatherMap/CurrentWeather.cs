using System.Text.Json.Serialization;
using McpServerAspNetCore.Models.OpenWeatherMap.Converters;

namespace McpServerAspNetCore.Models.OpenWeatherMap;

public class CurrentWeather
{
    [JsonPropertyName("coord")]
    public required Position Position { get; set; }

    [JsonPropertyName("weather")]
    public IEnumerable<WeatherCondition> Conditions { get; set; } = [];

    [JsonPropertyName("main")]
    public required CurrentWeatherDetail Detail { get; set; }

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("wind")]
    public required Wind Wind { get; set; }

    [JsonPropertyName("clouds")]
    public required Clouds Clouds { get; set; }

    [JsonPropertyName("sys")]
    public required Sun Sun { get; set; }

    [JsonPropertyName("dt")]
    [JsonConverter(typeof(UnixToDateTimeConverter))]
    public required DateTime Date { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("cod")]
    public int Code { get; set; }
}