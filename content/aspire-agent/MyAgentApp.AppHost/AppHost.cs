#if (UseAnyFoundry)
using Aspire.Hosting.Foundry;
#endif

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aspire-env");

// ── LLM Configuration ───────────────────────────────────────────────────────
#if (UseFoundry)
// Microsoft Foundry — model deployment declared in code, auto-provisioned by Aspire.
var foundry = builder.AddFoundry("foundry");
var chat = foundry.AddDeployment("chat", FoundryModel.OpenAI.Gpt4oMini);
#elif (UseFoundryLocal)
// Foundry Local — runs a local LLM, no Azure account needed.
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

var agent = builder.AddProject<Projects.MyAgentApp_Agent>("agent")
#if (UseAnyFoundry)
    .WithReference(chat)
    .WaitFor(chat)
#else
    .WithReference(openai)
#endif
    .WithUrlForEndpoint("https", url => url.Url = "/devui");

builder.Build().Run();
