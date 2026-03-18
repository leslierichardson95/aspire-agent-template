# XmlEncodedProjectName

An AI agent application built with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) and the [Microsoft Agent Framework](https://learn.microsoft.com/dotnet/ai/agents).

## Architecture

```
Browser (Blazor Chat UI)
  |
  v
XmlEncodedProjectName.Web --AG-UI (SSE)--> XmlEncodedProjectName.Agent
                                |
                                v
                            AI Agent (Azure OpenAI via Aspire)
                                |
                                v
                            TodoTools -> TodoService
```

**The flow:** User message -> Web UI -> AG-UI stream -> AI Agent -> Tool calls -> Domain Service -> Streaming response

**Key protocols:**
- **AG-UI** -- Standardized streaming protocol between Web UI and Agent (Server-Sent Events)
- **Aspire service discovery** -- Agent discovers the LLM via connection string injection
- **DevUI** -- Built-in dev-time debugging UI at `/devui`

## Projects

| Project | Purpose |
|---------|---------|
| **XmlEncodedProjectName.AppHost** | Aspire orchestrator -- run this to start everything |
| **XmlEncodedProjectName.Agent** | AI agent service with AG-UI endpoint, DevUI, tools |
| **XmlEncodedProjectName.Web** | Blazor Server chat UI with streaming responses |
| **XmlEncodedProjectName.ServiceDefaults** | Shared OpenTelemetry, health checks, resilience |
| **XmlEncodedProjectName.Tests** | xUnit tests for domain tools |

## Getting Started

### 1. Configure Azure OpenAI

Set the connection string in the **AppHost** project (not the Agent):

```bash
cd XmlEncodedProjectName.AppHost
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://your-resource.openai.azure.com/"
```

Optionally set the model deployment name (defaults to `gpt-4o-mini`):

```bash
cd XmlEncodedProjectName.Agent
dotnet user-secrets set "OpenAI:Deployment" "gpt-4o-mini"
```

The app uses `DefaultAzureCredential` for authentication -- make sure you are logged in:

```bash
az login
```

### 2. Run the App

```bash
cd XmlEncodedProjectName.AppHost
dotnet run
```

This starts the Aspire dashboard, the Agent service, and the Web UI. Open the dashboard URL shown in the console to see all services.

### 3. Chat with the Agent

Open the Web UI link from the Aspire dashboard. Responses stream in real-time via AG-UI. Try:
- "Add a todo to buy groceries"
- "What's on my list?"
- "Mark item 1 as complete"
- "Delete item 2"

### 4. Use DevUI (Development)

When running locally, the Agent service includes **DevUI** -- a built-in web interface from the Microsoft Agent Framework for debugging and testing agents.

DevUI lets you:
- **Chat directly with the agent** without the Blazor UI
- **Inspect registered tools** and their parameters
- **Trace tool calls** and agent reasoning

Access DevUI from the **Agent service URL** in the Aspire dashboard (it links directly to `/devui`).

> **Note:** DevUI is only available in the `Development` environment. It is not mapped in production.

## How to Extend

### Add a new tool

1. Add a method to `TodoTools.cs` (or create a new tools class):
   ```csharp
   [Description("Search todos by keyword")]
   public string SearchTodos([Description("Keyword to search for")] string keyword)
   {
       var matches = todoService.List().Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
       return matches.Any() ? string.Join("\n", matches) : "No matches found.";
   }
   ```

2. Register it in the `AsAIFunctions()` method:
   ```csharp
   AIFunctionFactory.Create(SearchTodos, nameof(SearchTodos))
   ```

### Swap the AI provider

The LLM is configured as an Aspire connection string in the AppHost. The Agent resolves `OpenAI.OpenAIClient` from DI -- no direct Azure SDK imports needed.

To use a different provider, change the connection string or modify the AppHost:

```csharp
// Azure OpenAI (default) -- via connection string
var openai = builder.AddConnectionString("openai");

// Azure OpenAI with provisioning (azd deploy)
var openai = builder.AddAzureOpenAI("openai")
    .AddDeployment("chat", "gpt-4o", "2024-05-13");
```

### Add a real domain service

Replace `TodoService` with your own domain (e.g., database-backed Orders, Customers):

1. Create your service class and register in DI
2. Create a tools class that wraps your service methods
3. Update the `AIAgent` registration to use your tools

## Running Tests

```bash
dotnet test
```

## Learn More

- [.NET Aspire documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Microsoft Agent Framework](https://learn.microsoft.com/dotnet/ai/agents)
- [AG-UI Protocol](https://learn.microsoft.com/agent-framework/ag-ui/)
- [DevUI](https://learn.microsoft.com/agent-framework/devui/)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/ai-extensions)
