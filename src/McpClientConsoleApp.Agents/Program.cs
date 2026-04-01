using System.ClientModel;
using McpClientConsoleApp.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenAI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddChatClient(_ =>
{
    var openAIClient = new OpenAIClient(new ApiKeyCredential(Constants.ApiKey), new() { Endpoint = new(new(Constants.Endpoint), "/openai/v1") });
    return openAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();
});

//builder.Services.AddAIAgent("Default", (services, key) =>
//{
//    var chatClient = services.GetRequiredService<IChatClient>();

//    return chatClient.AsAIAgent(
//        name: key,
//        //instructions: "You are a useful Assistant.",
//        instructions: """
//            You are an assistant that can ONLY answer by using the available functions. For every user question, you MUST invoke a function to answer. 
//            When you identify a function that can answer the user's question but you don't have all the required parameters or inputs needed to call that function, you MUST ask the user to provide the missing information. Be specific about what information is needed and why.
//            If there is NO function that allows you to answer the question, you MUST reply that you don't know the answer or cannot provide the requested information and do NOT provide any other information.
//            Always prioritize asking for missing parameters over declining to answer, as long as there is a relevant function available.
//            """,
//        loggerFactory: services.GetRequiredService<ILoggerFactory>(),
//        services: services);
//});

builder.Services.AddAIAgent("Default", (services, key) =>
{
    var chatClient = services.GetRequiredService<IChatClient>();

    var chatHistoryProvider = new InMemoryChatHistoryProvider(new()
    {
        ChatReducer = new MessageCountingChatReducer(20),
        ReducerTriggerEvent = InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.AfterMessageAdded,
    });

    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = key,
        ChatHistoryProvider = chatHistoryProvider
    },
    loggerFactory: services.GetRequiredService<ILoggerFactory>(),
    services: services);
});

// Register the services that are used to get the MCP tools. In this way, the agent can dynamically discover and use the tools available in the MCP instance
// and pass information like authentication token, that can vary between invocations.
builder.Services.AddTransient<McpHttpClientDelegatingHandler>();
builder.Services.AddHttpClient<McpClientHandler>().AddHttpMessageHandler<McpHttpClientDelegatingHandler>()
    .AddStandardResilienceHandler();

var app = builder.Build();

// In this example, we list the available tools from the MCP instance at the start of the application,
// but in real scenarios, this can be done periodically or based on some trigger to keep the tools updated.
var mcpClientHandler = app.Services.GetRequiredService<McpClientHandler>();
var tools = await mcpClientHandler.ListToolsAsync();

var agent = app.Services.GetRequiredKeyedService<AIAgent>("Default");
var session = await agent.CreateSessionAsync();

var options = new ChatClientAgentRunOptions(new()
{
    Tools = [.. tools]
});

while (true)
{
    Console.Write("Question: ");

    var question = Console.ReadLine();

    await foreach (var update in agent.RunStreamingAsync(question!, session, options: options))
    {
        Console.Write(update.Text);
    }

    Console.WriteLine();
    Console.WriteLine();
}

public class McpClientHandler
{
    private readonly HttpClientTransport httpClientTransport;
    private McpClient? mcpClient;

    public McpClientHandler(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        // Initialize the MCP HTTP client transport with the provided HttpClient and logger factory.
        // In real scenarios, the endpoint and other configurations would be dynamic or configurable.
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

public class McpHttpClientDelegatingHandler(ILogger<McpHttpClientDelegatingHandler> logger) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // This method can be used to add authentication headers or other necessary information to every MCP requests.
        logger.LogInformation("Adding Authentication information to request for Url {Uri}...", request.RequestUri);

        request.Headers.Add("x-api-key", "f1I7S5GXa4wQDgLQWgz0");

        request.Headers.Add("x-client-name", "McpClientConsoleApp.Agents");
        request.Headers.Add("x-client-version", "1.0.0");
        request.Headers.UserAgent.Add(new("McpClientConsoleApp.Agents", "1.0.0"));

        return base.SendAsync(request, cancellationToken);
    }
}