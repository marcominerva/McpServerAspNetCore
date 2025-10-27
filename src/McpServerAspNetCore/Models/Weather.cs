using System.ComponentModel;
using McpServerAspNetCore.Models.OpenWeatherMap;

namespace McpServerAspNetCore.Models;

public class Weather(CurrentWeather weather, UnitSystem units = UnitSystem.Metric)
{
    public string CityName { get; set; } = weather.Name;

    [Description("A single word that describe the condition")]
    public string Condition { get; set; } = weather.Conditions.First().Condition;

    public string ConditionIcon { get; set; } = weather.Conditions.First().ConditionIcon;

    public string ConditionIconUrl => $"https://openweathermap.org/img/w/{ConditionIcon}.png";

    [Description("A brief description of the condition")]
    public string ConditionDescription { get; set; } = weather.Conditions.First().Description;

    [Description("The current temperature, in Celsius degrees for Metric units or Fahrenheit for Imperial units")]
    public decimal Temperature { get; set; } = weather.Detail.Temperature;

    [Description("The unit system used for measurements")]
    public UnitSystem Units { get; set; } = units;
}
