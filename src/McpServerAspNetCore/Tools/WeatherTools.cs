using System.ComponentModel;
using System.Security.Claims;
using McpServerAspNetCore.Models;
using McpServerAspNetCore.Models.OpenWeatherMap;
using McpServerAspNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;

namespace McpServerAspNetCore.Tools;

[McpServerToolType]
public class WeatherTools
{
    [McpServerTool(Name = "get_current_weather", Title = "Get Current Weather", UseStructuredContent = true)]
    [Description("Get the current weather condition. This is the method to get weather of today")]
    public static async Task<Weather> GetCurrentWeatherAsync([Description("The city for which to get the current weather condition")] string city,
        [Description("The unit system to use (Metric or Imperial)"), DefaultValue(UnitSystem.Metric)] UnitSystem units = UnitSystem.Metric,
        [Description("The language code for weather descriptions (e.g., 'en', 'es', 'fr'). Infer from user language"), DefaultValue("en")] string language = "en",
        WeatherService weatherService = null!, CancellationToken cancellationToken = default)
    {
        var weather = await weatherService.GetCurrentWeatherAsync(city, units, language, cancellationToken);
        var response = new Weather(weather, units);

        return response;
    }

    [McpServerTool(Name = "get_weather_forecast", Title = "Get Weather Forecast", UseStructuredContent = true)]
    [Description("Get the weather condition for the next days. If you want to get the current condition, invoke the get_current_weather tool")]
    [Authorize]
    public static async Task<DailyForecastWeather> GetWeatherForecastAsync([Description("The city for which to get the weather forecast for the next days.")] string city,
        [Description("The number of days for which to return the forecast (from 1 to 16)")] int days,
        [Description("The unit system to use (Metric or Imperial)"), DefaultValue(UnitSystem.Metric)] UnitSystem units = UnitSystem.Metric,
        [Description("The language code for weather descriptions (e.g., 'en', 'es', 'fr'). Infer from user language"), DefaultValue("en")] string language = "en",
        WeatherService weatherService = null!, ClaimsPrincipal user = null!, ILogger<WeatherTools> logger = null!, CancellationToken cancellationToken = default)
    {
        var userName = user.Identity?.Name ?? "Unknown";
        logger.LogInformation("User {UserName} requested local time for city {City}", userName, city);

        var weather = await weatherService.GetWeatherForecastAsync(city, days, units, language, cancellationToken);
        return weather;
    }
}
