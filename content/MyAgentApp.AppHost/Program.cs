var builder = DistributedApplication.CreateBuilder(args);

// ── LLM Configuration ───────────────────────────────────────────────────────
// External connection string for Azure OpenAI. Set in user-secrets:
//   cd MyAgentApp.AppHost
//   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://your-resource.openai.azure.com/"
var openai = builder.AddConnectionString("openai");

var agent = builder.AddProject<Projects.MyAgentApp_Agent>("agent")
    .WithReference(openai)
    .WithUrlForEndpoint("https", url => url.Url = "/devui");

var web = builder.AddProject<Projects.MyAgentApp_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(agent)
    .WaitFor(agent);

builder.Build().Run();
