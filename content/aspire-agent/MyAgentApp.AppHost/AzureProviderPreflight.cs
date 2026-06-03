using Azure.Identity;
using Azure.ResourceManager;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Pre-flights the Azure resource providers Aspire/Foundry/ACA provisioning depends on.
/// Without these registered on the active subscription, provisioning fails mid-flight
/// with a generic "Failed to register resource provider … (Code: Conflict)" error.
/// This surfaces a single, actionable warning at AppHost startup instead.
/// Silently no-ops when there is no Azure auth available (e.g. OpenAI / Foundry-Local users).
/// </summary>
internal static class AzureProviderPreflight
{
    private static readonly string[] RequiredProviders =
    [
        "Microsoft.OperationalInsights",
        "Microsoft.Insights",
        "Microsoft.CognitiveServices",
        "Microsoft.App",
        "Microsoft.ContainerRegistry",
        "Microsoft.KeyVault",
        "Microsoft.Storage",
        "Microsoft.ManagedIdentity",
    ];

    public static IDistributedApplicationBuilder AddAzureProviderPreflight(this IDistributedApplicationBuilder builder)
    {
        if (builder.ExecutionContext.IsRunMode)
        {
            builder.Services.AddHostedService<PreflightService>();
        }
        return builder;
    }

    private sealed class PreflightService(ILogger<PreflightService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var client = new ArmClient(new DefaultAzureCredential());
                var sub = await client.GetDefaultSubscriptionAsync(stoppingToken);

                var missing = new List<string>();
                foreach (var name in RequiredProviders)
                {
                    var rp = await sub.GetResourceProviderAsync(name, cancellationToken: stoppingToken);
                    if (!string.Equals(rp.Value.Data.RegistrationState, "Registered", StringComparison.OrdinalIgnoreCase))
                    {
                        missing.Add(name);
                    }
                }

                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "⚠️  Azure resource providers not registered on subscription '{Subscription}': {Providers}. " +
                        "Foundry / Container Apps provisioning will fail until they are registered. Run:{NewLine}    az provider register -n {RegisterArgs}",
                        sub.Data.DisplayName,
                        string.Join(", ", missing),
                        Environment.NewLine,
                        string.Join(" -n ", missing));
                }
                else
                {
                    logger.LogDebug("All required Azure resource providers are registered on subscription '{Subscription}'.", sub.Data.DisplayName);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Azure resource provider preflight check skipped (could not authenticate or enumerate providers).");
            }
        }
    }
}
