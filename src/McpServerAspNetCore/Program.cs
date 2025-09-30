using System.ComponentModel;
using Microsoft.Net.Http.Headers;
using ModelContextProtocol.Server;
using SimpleAuthentication;
using SimpleAuthentication.ApiKey;
using SimpleAuthentication.JwtBearer;
using TinyHelpers.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddSimpleAuthentication(builder.Configuration)
    .AddMcp();

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new()
    {
        Name = "MCP Sample Server",
        Title = "MCP Sample Server",
        Version = "1.0.0"
    };
})
.WithHttpTransport()
.WithToolsFromAssembly();

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

app.MapMcp("/mcp")
.RequireAuthorization(policy =>
{
    policy.RequireAuthenticatedUser().AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme);
});

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
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

app.MapPost("api/auth/login", async (LoginRequest loginRequest, IJwtBearerService jwtBearerService) =>
{
    // Check for login rights...

    var token = await jwtBearerService.CreateTokenAsync(loginRequest.UserName);
    return TypedResults.Ok(new LoginResponse(token));
});

app.Run();

public record class LoginRequest(string UserName, string Password);

public record class LoginResponse(string Token);

[McpServerToolType]
public class ServerTools(IHttpContextAccessor httpContextAccessor, ILogger<ServerTools> logger)
{
    [McpServerTool]
    [Description("Returns the current date and time in UTC format.")]
    public DateTime GetUtcNow() => DateTime.UtcNow;

    [McpServerTool]
    [Description("Returns the current date and time of the specified time zone")]
    public DateTime GetLocalNow([Description("The time zone in the IANA format")] string timeZone)
    {
        var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous";
        logger.LogInformation("User {User} requested local time for time zone {TimeZone}", userName, timeZone);

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        return TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
    }
}
