using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServerAspNetCore.Tools;

[McpServerToolType]
public class DateTimeTools
{
    [McpServerTool(Name = "get_utc_now", Title = "Returns the current date and time in UTC format", UseStructuredContent = true, Idempotent = true, ReadOnly = true)]
    [Description("Returns the current date and time in UTC format")]
    [McpMeta("category", "Date and Time")]
    public static DateTime GetUtcNow() => DateTime.UtcNow;
}