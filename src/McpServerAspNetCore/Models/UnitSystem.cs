namespace McpServerAspNetCore.Models;

/// <summary>
/// Specifies the unit system for weather measurements.
/// </summary>
public enum UnitSystem
{
    /// <summary>
    /// Metric units (Celsius, meters/sec, mm).
    /// </summary>
    Metric,

    /// <summary>
    /// Imperial units (Fahrenheit, miles/hour, inches).
    /// </summary>
    Imperial
}
