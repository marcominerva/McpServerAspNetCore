using System.Text.Json.Serialization;
using McpServerAspNetCore.Models;
using McpServerAspNetCore.Services;
using Microsoft.Net.Http.Headers;
using SimpleAuthentication;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<WeatherService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("AppSettings:OpenWeatherMapBaseUrl")!);
});

// Configure JSON options to serialize enums as strings globally.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSimpleAuthentication(builder.Configuration).AddMcp();

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new() { Name = "MCP Sample Server", Version = "1.0.0" };
    options.ServerInstructions = "You are a helpful assistant that provides weather, date and time information.";
})
.AddAuthorizationFilters()  // <-- Enable Authorize attribute in MCP tools
.WithHttpTransport().WithToolsFromAssembly();   // <-- Tools are defined in the Tools folder in this project

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

builder.Services.AddDefaultProblemDetails();
builder.Services.AddDefaultExceptionHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp("/mcp");

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

app.MapGet("/api/weather/current", async (string city, UnitSystem units = UnitSystem.Metric, string language = "en",
    WeatherService weatherService = null!, CancellationToken cancellationToken = default) =>
{
    var weather = await weatherService.GetCurrentWeatherAsync(city, units, language, cancellationToken);
    var response = new Weather(weather, units);

    return TypedResults.Ok(response);
});

app.MapGet("/api/weather/daily", async (string city, int days, UnitSystem units = UnitSystem.Metric, string language = "en",
    WeatherService weatherService = null!, CancellationToken cancellationToken = default) =>
{
    var weather = await weatherService.GetWeatherForecastAsync(city, days, units, language, cancellationToken);
    return TypedResults.Ok(weather);
})
.RequireAuthorization();

app.Run();
