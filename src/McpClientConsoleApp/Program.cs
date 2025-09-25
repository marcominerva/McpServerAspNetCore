using System.Text;
using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;

var hostBuilder = Host.CreateApplicationBuilder(args);

var azureOpenAIClient = new AzureOpenAIClient(new Uri(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

hostBuilder.Services.AddChatClient(azureChatClient)
    .UseFunctionInvocation();

var app = hostBuilder.Build();

var transport = new SseClientTransport(
    new()
    {
        Endpoint = new Uri("https://localhost:7133/mcp"),
        Name = "Test MCP server",
        AdditionalHeaders = new Dictionary<string, string>
        {
            ["x-api-key"] = "f1I7S5GXa4wQDgLQWgz0"
        }
    });

await using var mcpClient = await McpClientFactory.CreateAsync(transport);
var tools = await mcpClient.ListToolsAsync();

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

    history.Add(new(ChatRole.Assistant, answer.ToString()));

    Console.WriteLine();
    Console.WriteLine();
}