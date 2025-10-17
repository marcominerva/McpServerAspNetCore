using System.Text;
using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;

var builder = Host.CreateApplicationBuilder(args);

var azureOpenAIClient = new AzureOpenAIClient(new(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

// Add the chat client and AI agent to the service collection.
builder.Services.AddChatClient(azureChatClient);
builder.Services.AddSingleton<AIAgent>(services => new ChatClientAgent(
    chatClient: services.GetRequiredService<IChatClient>(),
    options: new("You are a useful Assistant."),
    services: services));

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

var app = builder.Build();

var agent = app.Services.GetRequiredService<AIAgent>();

// Without depentency injection.
//var agent = azureChatClient.CreateAIAgent();

var history = new List<ChatMessage>();

var options = new ChatClientAgentRunOptions(new()
{
    Tools = [.. tools]
});

while (true)
{
    Console.Write("Question: ");

    var question = Console.ReadLine();
    history.Add(new(ChatRole.User, question!));

    var answer = new StringBuilder();

    await foreach (var update in agent.RunStreamingAsync(history, options: options))
    {
        Console.Write(update.Text);
        answer.Append(update.Text);
    }

    history.Add(new(ChatRole.Assistant, answer.ToString()));

    Console.WriteLine();
    Console.WriteLine();
}
