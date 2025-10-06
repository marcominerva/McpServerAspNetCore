using System.ComponentModel;
using System.Security.Claims;
using Microsoft.Net.Http.Headers;
using ModelContextProtocol.Server;
using SimpleAuthentication;
using TinyHelpers.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddSimpleAuthentication(builder.Configuration)
    .AddMcp();

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new() { Name = "MCP Sample Server", Version = "1.0.0" };
    options.ServerInstructions = "You are a helpful assistant that provides date and time information.";
})
.WithHttpTransport().WithToolsFromAssembly();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true)
            .AllowCredentials().WithExposedHeaders(HeaderNames.ContentDisposition);
    });
});

builder.Services.AddOpenApi(options =>
{
    options.RemoveServerList();
    options.AddDefaultProblemDetailsResponse();

    options.AddSimpleAuthentication(builder.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp("/mcp").RequireAuthorization();

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources, ClaimsPrincipal user) =>
{
    var endpoints = endpointSources.SelectMany(source => source.Endpoints);
    var routes = endpoints.OfType<RouteEndpoint>()
        .Select(e => new
        {
            Pattern = e.RoutePattern.RawText,
            Methods = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods
        });

    return routes;
});

app.Run();

[McpServerToolType]
public class DateTimeTools
{
    [McpServerTool(Name = "get_utc_now", Title = "Returns the current date and time in UTC format")]
    [Description("Returns the current date and time in UTC format")]
    public static DateTime GetUtcNow() => DateTime.UtcNow;

    [McpServerTool(Name = "get_local_now", Title = "Returns the current date and time in the specified time zone")]
    [Description("Returns the current date and time in the specified time zone")]
    public static DateTime GetLocalNow([Description("The time zone in IANA format")] string timeZone, IHttpContextAccessor httpContextAccessor, ILogger<DateTimeTools> logger)
    {
        var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous";
        logger.LogInformation("User {UserName} requested local time for time zone {TimeZone}", userName, timeZone);

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        return TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
    }
}