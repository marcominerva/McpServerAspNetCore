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

### Azure OpenAI Configuration (for Client Apps)

The sample client applications (`McpClientConsoleApp.ExtensionsAI` and `McpClientConsoleApp.Agents`) use Azure OpenAI to demonstrate how to integrate MCP tools with AI agents. To run these clients, you need to configure your Azure OpenAI credentials:

1. Create an [Azure OpenAI resource](https://portal.azure.com/#create/Microsoft.CognitiveServicesOpenAI) in the Azure Portal
2. Deploy a GPT model (e.g., GPT-4) from the Azure OpenAI Studio
3. Get your endpoint URL and API key from the Azure Portal (under "Keys and Endpoint")
4. Update the `Constants.cs` file in each client project

#### Update Constants.cs

Open `src/McpClientConsoleApp.ExtensionsAI/Constants.cs` (or `src/McpClientConsoleApp.Agents/Constants.cs`) and update the values:

```csharp
public static class Constants
{
    public const string Endpoint = "https://YOUR_RESOURCE_NAME.openai.azure.com/";
    public const string DeploymentName = "YOUR_DEPLOYMENT_NAME";
    public const string ApiKey = "YOUR_AZURE_OPENAI_API_KEY";
}
```

**Note:** For production applications, consider using Azure Key Vault or environment variables to store these sensitive credentials securely instead of hardcoding them.
