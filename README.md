# MCP Server with ASP.NET Core

Minimal implementation of a Model Context Protocol (MCP) server based on ASP.NET Core and .NET9. The project serves as an example to expose MCP tools to compatible clients, including authentication, automatic discovery, and API documentation.

## Project purpose
Provide a simple, ready-to-use starting point to build an MCP server on ASP.NET Core, useful for integrating server-side tools into agents/assistants that speak MCP.

## Exposed features
- Framework: ASP.NET Core on .NET 9, minimal configuration ready for extension.
- Security: access protected via API Key.
- MCP Tool Discovery: automatic detection/exposure of the server's available MCP tools.
- OpenAPI/Swagger UI: interface to explore and test the exposed endpoints.
- CORS: enabled/configurable to allow access from secure external origins.
- Example server-side tools:
 - Weather: retrieve weather conditions for a location.
 - Time: get current date and time (e.g., for a specific time zone).

These features make it possible to connect an MCP client and automatically discover/consume the tools published by the server, with support for security, documentation, and interoperability.

## Configuration

### OpenWeatherMap API Key

The weather tools use the [OpenWeatherMap API](https://openweathermap.org/api) to retrieve weather data. To use these tools, you need to obtain a free API key:

1. Go to [OpenWeatherMap](https://openweathermap.org/) and create a free account
2. Navigate to your [API keys page](https://home.openweathermap.org/api_keys)
3. Generate a new API key (or copy your existing one)
4. Add the API key to your configuration file

#### Update appsettings.json

Open `src/McpServerAspNetCore/appsettings.json` and update the `OpenWeatherMapAppId` value:

```json
{
    "AppSettings": {
        "OpenWeatherMapBaseUrl": "https://api.openweathermap.org/data/2.5/",
        "OpenWeatherMapAppId": "YOUR_API_KEY_HERE"
    }
}
```
