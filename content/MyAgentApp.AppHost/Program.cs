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

var agent = builder.AddProject<Projects.MyAgentApp_Agent>("agent")
    .WithReference(openai)
    .WithUrlForEndpoint("https", url => url.Url = "/devui");

var web = builder.AddProject<Projects.MyAgentApp_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(agent)
    .WaitFor(agent);

builder.Build().Run();
