using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

var azureOpenAIClient = new AzureOpenAIClient(new(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

var agent = azureChatClient.CreateAIAgent(
    instructions: "You are a useful Assistant.",
    name: "ChatClientAgent");

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

var thread = agent.GetNewThread();

var options = new ChatClientAgentRunOptions(new()
{
    //Tools = [.. tools]
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