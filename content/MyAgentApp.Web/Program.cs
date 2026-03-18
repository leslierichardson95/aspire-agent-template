using Microsoft.Agents.AI.AGUI;
using MyAgentApp.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register an HttpClient for the agent service (Aspire service discovery)
builder.Services.AddHttpClient("AgentApi", client =>
{
    client.BaseAddress = new Uri("https+http://agent");
    client.Timeout = TimeSpan.FromSeconds(120);
});

// Register the AG-UI chat client for streaming agent communication
builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("AgentApi");
    return new AGUIChatClient(httpClient, httpClient.BaseAddress + "api/agui");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
