using System.ComponentModel;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp.ExtensionsAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

var builder = Host.CreateApplicationBuilder(args);

var azureOpenAIClient = new AzureOpenAIClient(new Uri(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

builder.Services.AddChatClient(azureChatClient).UseFunctionInvocation();

builder.Services.AddTransient<McpHttpClientDelegatingHandler>();
builder.Services.AddHttpClient<McpClientHandler>().AddHttpMessageHandler<McpHttpClientDelegatingHandler>()
    .AddStandardResilienceHandler();

var app = builder.Build();

var mcpClientHandler = app.Services.GetRequiredService<McpClientHandler>();
var tools = await mcpClientHandler.ListToolsAsync();

var chat = app.Services.GetRequiredService<IChatClient>();
var history = new List<ChatMessage>();

while (true)
{
    Console.Write("Question: ");

    var question = Console.ReadLine();
    history.Add(new(ChatRole.User, question!));

    var answer = new StringBuilder();

    await foreach (var update in chat.GetStreamingResponseAsync(history, new() { Tools = [.. tools] }))
    {
        Console.Write(update.Text);
        answer.Append(update.Text);
    }

    //var response = await chat.GetResponseAsync<DateTimeInformation>(history, options: new() { Tools = [.. tools] });
    //Console.WriteLine(response.Text);
    //answer.AppendLine(response.Text);

    history.Add(new(ChatRole.Assistant, answer.ToString()));

    Console.WriteLine();
    Console.WriteLine();
}

public record class DateTimeInformation([property: Description("The current date and time, including the timezone offset")] DateTimeOffset DateTime,
    [property: Description("The timezone in Windows format. If the response is in IANA format, convert it to Windows format.")] string TimeZone);

public class McpClientHandler
{
    private readonly HttpClientTransport httpClientTransport;
    private McpClient? mcpClient;

    public McpClientHandler(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        httpClientTransport = new HttpClientTransport(new()
        {
            Endpoint = new("https://localhost:7133/mcp"),
            Name = "Test MCP client"
        }, httpClient, loggerFactory);
    }

    public async Task<IEnumerable<McpClientTool>> ListToolsAsync()
    {
        mcpClient ??= await McpClient.CreateAsync(httpClientTransport);
        return await mcpClient.ListToolsAsync();
    }
}

public class McpHttpClientDelegatingHandler(ILogger<McpHttpClientDelegatingHandler> logger) :  DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding Auth information to request for Url {Uri}...", request.RequestUri);

        request.Headers.Add("x-api-key", "f1I7S5GXa4wQDgLQWgz0");

        request.Headers.Add("x-client-name", "McpClientConsoleApp.ExtensionsAI");
        request.Headers.Add("x-client-version", "1.0.0");
        request.Headers.Add("User-Agent", "McpClientConsoleApp.ExtensionsAI/1.0.0");

        return base.SendAsync(request, cancellationToken);
    }
}