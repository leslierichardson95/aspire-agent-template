# Aspire AI Agent

A minimal AI agent service built with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) and the [Microsoft Agent Framework](https://learn.microsoft.com/dotnet/ai/agents). This template provides the foundation — an Aspire-orchestrated agent with DevUI for testing — so you can focus on adding your own tools and domain logic.

## Why Aspire for AI Agents?

.NET Aspire turns a multi-service agent system into a single `dotnet run` experience:

- **One-command startup** — The AppHost launches the agent, connects the LLM, and wires up service discovery automatically
- **Observability built in** — OpenTelemetry traces every agent → LLM → tool call across services; view logs, traces, and metrics in the Aspire dashboard
- **Swap providers instantly** — Change the LLM (Foundry, Azure OpenAI, OpenAI, local) by changing one connection string — no code changes
- **Resilience for AI workloads** — ServiceDefaults configures retry and circuit-breaker policies tuned for LLM call latencies
- **DevUI included** — Built-in chat and tool inspection UI for debugging agents during development
- **Cloud-ready deployment** — `azd up` deploys the entire distributed agent system to Azure

## Architecture

```
XmlEncodedProjectName.AppHost (Aspire orchestrator)
    |
    v
<!--#if (UseFoundry) -->
XmlEncodedProjectName.Agent --> Azure AI Foundry (auto-provisioned)
<!--#elif (UseFoundryLocal) -->
XmlEncodedProjectName.Agent --> Foundry Local (local LLM)
<!--#elif (UseAzureOpenAI) -->
XmlEncodedProjectName.Agent --> Azure OpenAI (connection string)
<!--#else -->
XmlEncodedProjectName.Agent --> OpenAI API (connection string)
<!--#endif -->
    |
    v
DevUI (/devui) -- built-in chat & debugging interface
```

## Projects

| Project | Purpose |
|---------|---------|
| **XmlEncodedProjectName.AppHost** | Aspire orchestrator — run this to start everything |
| **XmlEncodedProjectName.Agent** | AI agent service with DevUI for testing |
| **XmlEncodedProjectName.ServiceDefaults** | Shared OpenTelemetry, health checks, resilience |

## Getting Started

<!--#if (UseFoundry) -->
### 1. Configure Azure AI Foundry

The model deployment is declared in the AppHost — `azd up` will provision it automatically.

For local development, make sure you're logged in:

```bash
az login
```

<!--#elif (UseFoundryLocal) -->
### 1. Install Foundry Local

Install Foundry Local for zero-config local LLM:
https://learn.microsoft.com/azure/ai-foundry/foundry-local/get-started

No Azure account or API keys needed.

<!--#elif (UseAzureOpenAI) -->
### 1. Configure Azure OpenAI

Set the connection string in the **AppHost** project:

```bash
cd XmlEncodedProjectName.AppHost
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://your-resource.openai.azure.com/"
```

Make sure you're logged in: `az login`

<!--#else -->
### 1. Configure OpenAI

Set the connection string in the **AppHost** project:

```bash
cd XmlEncodedProjectName.AppHost
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://api.openai.com/v1;Key=sk-your-key"
```

For **GitHub Models**: `"Endpoint=https://models.inference.ai.azure.com;Key=ghp_your-token"`

<!--#endif -->

### 2. Run

```bash
cd XmlEncodedProjectName.AppHost
dotnet run
```

Open the Aspire dashboard URL shown in the console. Click the agent's endpoint to open DevUI.

### 3. Chat

DevUI provides a chat interface for testing your agent. Try asking anything — the agent is a general-purpose assistant by default.

## Next Steps

This is a starting point. To build a real application:

- **Add tools** — Create a tools class with `[Description]` attributes, register with `AsAIFunctions()`
- **Add a web UI** — Use `dotnet new aspire-agent-starter --IncludeWeb` for a Blazor chat frontend
- **Add MCP tools** — Use `dotnet new aspire-agent-starter --IncludeMcp` for external tool hosting
- **Add multi-agent handoff** — Use `dotnet new aspire-agent-starter --IncludeHandoff` for Router/Specialist patterns

Or see the full template: `dotnet new aspire-agent-starter`

## Learn More

- [.NET Aspire documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Microsoft Agent Framework](https://learn.microsoft.com/dotnet/ai/agents)
- [DevUI](https://learn.microsoft.com/agent-framework/devui/)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/ai-extensions)
