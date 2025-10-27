using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServerAspNetCore.Tools;

[McpServerToolType]
public class DateTimeTools
{
    [McpServerTool(Name = "get_utc_now", Title = "Returns the current date and time in UTC format", UseStructuredContent = true)]
    [Description("Returns the current date and time in UTC format")]
    [McpMeta("category", "Date and Time")]
    public static DateTime GetUtcNow() => DateTime.UtcNow;

    [McpServerTool(Name = "get_local_now", Title = "Returns the current date and time in the specified time zone", UseStructuredContent = true)]
    [Description("Returns the current date and time in the specified time zone")]
    [McpMeta("category", "Date and Time")]
    public static DateTime GetLocalNow([Description("The time zone in IANA format")] string timeZone, ILogger<DateTimeTools> logger)
    {
        logger.LogInformation("Requesting local time for time zone {TimeZone}", timeZone);

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        return TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
    }
}