using Azure;
using Azure.AI.OpenAI;
using McpClientConsoleApp.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

var azureOpenAIClient = new AzureOpenAIClient(new(Constants.Endpoint), new AzureKeyCredential(Constants.ApiKey));
var azureChatClient = azureOpenAIClient.GetChatClient(Constants.DeploymentName).AsIChatClient();

var agent = azureChatClient.CreateAIAgent(
    instructions: """
        You are an assistant that can ONLY answer by using the available functions. For every user question, you MUST invoke a function to answer. 
        When you identify a function that can answer the user's question but you don't have all the required parameters or inputs needed to call that function, you MUST ask the user to provide the missing information. Be specific about what information is needed and why.
        If there is NO function that allows you to answer the question, you MUST reply that you don't know the answer or cannot provide the requested information and do NOT provide any other information.
        Always prioritize asking for missing parameters over declining to answer, as long as there is a relevant function available.
        """,
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