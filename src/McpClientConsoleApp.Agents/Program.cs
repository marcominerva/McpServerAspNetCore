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

// Add a chat client to the service collection.
builder.Services.AddChatClient((_) => new AzureOpenAIClient(new Uri(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey))
        .GetChatClient(Constants.DeploymentName)
        .AsIChatClient());

// Add the AI agent to the service collection.
builder.Services.AddSingleton<AIAgent>(services => new ChatClientAgent(
    chatClient: services.GetRequiredService<IChatClient>(),
    options: new("You are a useful Assistant."),
    services: services));

var transport = new HttpClientTransport(
    new()
    {
        Endpoint = new Uri("https://localhost:7133/mcp"),
        Name = "Test MCP server",
        AdditionalHeaders = new Dictionary<string, string>
        {
            ["x-api-key"] = "f1I7S5GXa4wQDgLQWgz0"
        }
    });

await using var mcpClient = await McpClient.CreateAsync(transport);
var tools = await mcpClient.ListToolsAsync();

var app = builder.Build();

var chat = app.Services.GetRequiredService<AIAgent>();
var history = new List<ChatMessage>();

var options = new ChatClientAgentRunOptions(new()
{
    Tools = [.. tools.Cast<AITool>()]
});

while (true)
{
    Console.Write("Question: ");

    var question = Console.ReadLine();
    history.Add(new(ChatRole.User, question!));

    var answer = new StringBuilder();

    await foreach (var update in chat.RunStreamingAsync(history, options: options))
    {
        Console.Write(update.Text);
        answer.Append(update.Text);
    }

    history.Add(new(ChatRole.Assistant, answer.ToString()));

    Console.WriteLine();
    Console.WriteLine();
}