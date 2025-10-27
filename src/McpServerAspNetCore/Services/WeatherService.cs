using McpServerAspNetCore.Models;
using McpServerAspNetCore.Models.OpenWeatherMap;

namespace McpServerAspNetCore.Services;

/// <summary>
/// Provides methods to retrieve weather data from the OpenWeatherMap API.
/// </summary>
/// <remarks>
/// This service acts as a client for the OpenWeatherMap API, offering methods to fetch current weather conditions
/// and daily forecasts for specified cities. Temperature values are returned in the specified unit system (metric or imperial).
/// For more information about the OpenWeatherMap API, see <see href="https://openweathermap.org/api"/>.
/// </remarks>
public class WeatherService(HttpClient httpClient, IConfiguration configuration)
{
    private readonly string appId = configuration.GetValue<string>("AppSettings:OpenWeatherMapAppId")!;

    /// <summary>
    /// Retrieves the current weather conditions for a specified city.
    /// </summary>
    /// <param name="city">The name of the city for which to retrieve current weather data.</param>
    /// <param name="units">The unit system to use for temperature and other measurements (Metric or Imperial). Defaults to Metric.</param>
    /// <param name="language">The language code for weather descriptions. Defaults to English ("en").</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="CurrentWeather"/> data for the specified city.
    /// </returns>
    /// <remarks>
    /// This method queries the OpenWeatherMap current weather endpoint and returns real-time weather information
    /// including temperature, humidity, wind conditions, and cloud coverage. Temperature is returned in Celsius for metric units or Fahrenheit for imperial units.
    /// </remarks>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or the city is not found.</exception>
    public async Task<CurrentWeather> GetCurrentWeatherAsync(string city, UnitSystem units = UnitSystem.Metric, string language = "en", CancellationToken cancellationToken = default)
    {
        var unitsParam = units.ToString().ToLowerInvariant();
        var response = await httpClient.GetFromJsonAsync<CurrentWeather>($"weather?q={city}&appid={appId}&units={unitsParam}&lang={language}", cancellationToken)
            .ConfigureAwait(false);

        return response!;
    }

    /// <summary>
    /// Retrieves the daily weather forecast for a specified city and number of days.
    /// </summary>
    /// <param name="city">The name of the city for which to retrieve the weather forecast.</param>
    /// <param name="days">The number of forecast days to retrieve. This value is automatically clamped between 1 and 16.</param>
    /// <param name="units">The unit system to use for temperature and other measurements (Metric or Imperial). Defaults to Metric.</param>
    /// <param name="language">The language code for weather descriptions. Defaults to English ("en").</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="DailyForecastWeather"/> data for the specified city and time range.
    /// </returns>
    /// <remarks>
    /// This method queries the OpenWeatherMap daily forecast endpoint to retrieve weather predictions for up to 16 days.
    /// The <paramref name="days"/> parameter is automatically clamped to ensure it falls within the valid range (1-16),
    /// regardless of the value provided by the caller. Temperature is returned in Celsius for metric units or Fahrenheit for imperial units.
    /// </remarks>
    /// <exception cref="HttpRequestException">Thrown when the API request fails or the city is not found.</exception>
    public async Task<DailyForecastWeather> GetWeatherForecastAsync(string city, int days, UnitSystem units = UnitSystem.Metric, string language = "en", CancellationToken cancellationToken = default)
    {
        var clampedDays = Math.Clamp(days, 1, 16);
        var unitsParam = units.ToString().ToLowerInvariant();

        var response = await httpClient.GetFromJsonAsync<DailyForecastWeather>($"forecast/daily?q={city}&cnt={clampedDays}&appid={appId}&units={unitsParam}&lang={language}", cancellationToken)
            .ConfigureAwait(false);

        return response!;
    }
}
