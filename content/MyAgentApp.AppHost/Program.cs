var builder = DistributedApplication.CreateBuilder(args);

// ── LLM Configuration ───────────────────────────────────────────────────────
#if (UseFoundry)
// Azure AI Foundry connection string. Set in user-secrets:
//   cd MyAgentApp.AppHost
//   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://your-foundry-endpoint.openai.azure.com/"
#elif (UseAzureOpenAI)
// Azure OpenAI connection string. Set in user-secrets:
//   cd MyAgentApp.AppHost
//   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://your-resource.openai.azure.com/"
#else
// OpenAI API connection string. Set in user-secrets:
//   cd MyAgentApp.AppHost
//   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://api.openai.com/v1;Key=sk-your-key"
//   For GitHub Models: "Endpoint=https://models.inference.ai.azure.com;Key=ghp_your-token"
#endif
var openai = builder.AddConnectionString("openai");

#if (IncludeMcp)
// ── MCP Server ───────────────────────────────────────────────────────────────
// The MCP server hosts domain tools accessible via the Model Context Protocol.
// The agent discovers and invokes these tools automatically at startup.
var mcp = builder.AddProject<Projects.MyAgentApp_Mcp>("mcp-server");
#endif

var agent = builder.AddProject<Projects.MyAgentApp_Agent>("agent")
    .WithReference(openai)
#if (IncludeMcp)
    .WithReference(mcp)
#endif
    .WithUrlForEndpoint("https", url => url.Url = "/devui");

var web = builder.AddProject<Projects.MyAgentApp_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(agent)
    .WaitFor(agent);

builder.Build().Run();
