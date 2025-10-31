using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

var builder = Host.CreateApplicationBuilder(args);

// Add the chat client and AI agent to the service collection.
builder.Services.AddSingleton(services =>
{
    var azureOpenAIClient = new AzureOpenAIClient(new(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
    var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

    return azureChatClient.CreateAIAgent(
        instructions: "You are a useful Assistant.",
        name: "ChatClientAgent",
        loggerFactory: services.GetRequiredService<ILoggerFactory>(),
        services: services);
});

var app = builder.Build();

var transport = new HttpClientTransport(new()
{
    Endpoint = new("https://localhost:7133/mcp"),
    Name = "Test MCP client",
    AdditionalHeaders = new Dictionary<string, string>
    {
        ["x-api-key"] = "f1I7S5GXa4wQDgLQWgz0"
    }
});

await using var mcpClient = await McpClient.CreateAsync(transport);
var tools = await mcpClient.ListToolsAsync();

var agent = app.Services.GetRequiredService<ChatClientAgent>();
var thread = agent.GetNewThread();

var options = new ChatClientAgentRunOptions(new()
{
    Tools = [.. tools]
});

while (true)
{
    Console.Write("Question: ");

    var question = Console.ReadLine();

    await foreach (var update in agent.RunStreamingAsync(question!, thread, options: options))
    {
        Console.Write(update.Text);
    }

    Console.WriteLine();
    Console.WriteLine();
}