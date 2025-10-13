using System.ComponentModel;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp.ExtensionsAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;

var builder = Host.CreateApplicationBuilder(args);

var azureOpenAIClient = new AzureOpenAIClient(new Uri(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

builder.Services.AddChatClient(azureChatClient).UseFunctionInvocation();

var transport = new HttpClientTransport(new()
{
    Endpoint = new Uri("https://localhost:7133/mcp"),
    Name = "Test MCP client",
    AdditionalHeaders = new Dictionary<string, string>
    {
        ["x-api-key"] = "f1I7S5GXa4wQDgLQWgz0"
    }
});

await using var mcpClient = await McpClient.CreateAsync(transport);
var tools = await mcpClient.ListToolsAsync();

var app = builder.Build();

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