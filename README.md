# aspire-agent-template

A `dotnet new` template that scaffolds a .NET Aspire solution with an AI agent service, Blazor chat UI, and sample tests.

## What You Get

```
dotnet new aspire-agent -n InterviewCoach
```

Creates:

```
InterviewCoach/
├── InterviewCoach.AppHost/        # Aspire orchestrator
├── InterviewCoach.ServiceDefaults/# Shared OpenTelemetry + health checks
├── InterviewCoach.Agent/          # AI agent API service
│   ├── Program.cs                 # Agent wiring + /api/chat endpoint
│   └── SampleTools.cs             # Example function tools
├── InterviewCoach.Web/            # Blazor Server chat UI
│   └── Components/Pages/Home.razor
├── InterviewCoach.Tests/          # xUnit tests for tools
└── InterviewCoach.sln
```

## Quick Start

### 1. Install the template

```bash
dotnet new install path/to/aspire-agent-template/content
```

### 2. Create a project

```bash
dotnet new aspire-agent -n MyAgent
cd MyAgent
```

### 3. Configure Azure OpenAI

```bash
cd MyAgent.Agent
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com"
dotnet user-secrets set "AzureOpenAI:Deployment" "gpt-4o-mini"
```

### 4. Run with Aspire

```bash
cd MyAgent.AppHost
dotnet run
```

The Aspire dashboard opens automatically, showing both the Agent API and Web UI services.

## Template Structure

| Project | Purpose |
|---------|---------|
| **AppHost** | Aspire orchestrator — wires Agent + Web with service discovery |
| **ServiceDefaults** | Shared config: OpenTelemetry traces/metrics, health checks, resilience |
| **Agent** | ASP.NET Core API with AI agent (Microsoft Agent Framework RC) |
| **Web** | Blazor Server app with chat UI, calls Agent via service discovery |
| **Tests** | xUnit tests for the sample tools |

## Customizing

### Add Your Own Tools

Edit `SampleTools.cs` (or create new tool classes):

```csharp
[Description("Searches the knowledge base")]
public static string SearchKnowledge(
    [Description("The search query")] string query)
{
    // Your logic here
    return "Results...";
}
```

Then register them in `Program.cs`:

```csharp
tools: SampleTools.AsAIFunctions()
```

### Change LLM Provider

The template uses Azure OpenAI by default. To use OpenAI directly, modify `Program.cs`:

```csharp
builder.Services.AddSingleton<IChatClient>(sp =>
{
    return new OpenAIClient(apiKey)
        .GetChatClient("gpt-4o-mini")
        .AsIChatClient();
});
```

## Tech Stack

- **.NET 10** + Aspire 9.2
- **Microsoft Agent Framework** (RC) — `Microsoft.Agents.AI.OpenAI`
- **Microsoft.Extensions.AI** — AI abstractions
- **Azure.Identity** — `AzureCliCredential` for local dev
- **Blazor Server** — interactive chat UI

## Requirements

- .NET 10 SDK
- Aspire workload (`dotnet workload install aspire`)
- Docker (for Aspire orchestration)
- Azure OpenAI resource (or modify for OpenAI/Ollama)

## License

MIT
