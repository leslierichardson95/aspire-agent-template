#if (UseAnyFoundry)
using Aspire.Hosting.Foundry;
#endif

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aspire-env");

// ── LLM Configuration ───────────────────────────────────────────────────────
#if (UseFoundry)
// Microsoft Foundry — model deployment declared in code, auto-provisioned by Aspire.
// No manual user-secrets needed for run mode; Aspire injects connection info automatically.
var foundry = builder.AddFoundry("foundry");
var chat = foundry.AddDeployment("chat", FoundryModel.OpenAI.Gpt4oMini);
#elif (UseFoundryLocal)
// Foundry Local — runs a local LLM, no Azure account needed.
// Requires Foundry Local installed: https://learn.microsoft.com/azure/ai-foundry/foundry-local/get-started
var foundry = builder.AddFoundry("foundry")
    .RunAsFoundryLocal();
var chat = foundry.AddDeployment("chat", FoundryModel.Local.Phi4);
#elif (UseAzureOpenAI)
// Azure OpenAI — Aspire prompts for the endpoint in the dashboard if not configured.
// Can also be set via user-secrets: Parameters:openai-endpoint
var openaiEndpoint = builder.AddParameter("openai-endpoint");
var openai = builder.AddConnectionString("openai",
    b => b.Append($"{openaiEndpoint}"));
#else
// OpenAI API — Aspire prompts for endpoint and key in the dashboard if not configured.
// For GitHub Models, use endpoint: https://models.inference.ai.azure.com
var openaiEndpoint = builder.AddParameter("openai-endpoint");
var openaiKey = builder.AddParameter("openai-key", secret: true);
var openai = builder.AddConnectionString("openai",
    b => b.Append($"Endpoint={openaiEndpoint};Key={openaiKey}"));
#endif

#if (IncludeMcp)
// ── MCP Server ───────────────────────────────────────────────────────────────
// The MCP server hosts domain tools accessible via the Model Context Protocol.
// The agent discovers and invokes these tools automatically at startup.
var mcp = builder.AddProject<Projects.MyAgentApp_Mcp>("mcp-server");
#endif

var agent = builder.AddProject<Projects.MyAgentApp_Agent>("agent")
#if (UseAnyFoundry)
    .WithReference(chat)
    .WaitFor(chat)
#else
    .WithReference(openai)
#endif
#if (IncludeMcp)
    .WithReference(mcp)
    .WaitFor(mcp)
#endif
    .WithUrlForEndpoint("https", url => url.Url = "/devui");

#if (IncludeWeb)
var web = builder.AddProject<Projects.MyAgentApp_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(agent)
    .WaitFor(agent);
#endif

builder.Build().Run();
