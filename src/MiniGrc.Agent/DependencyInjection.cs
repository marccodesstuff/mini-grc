using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniGrc.Agent;
using MiniGrc.Agent.Knowledge;
using MiniGrc.Agent.Llm;

namespace MiniGrc.Agent;

/// <summary>
/// Composition root for the agent layer. Registers the <see cref="ComplianceAgentService"/> and,
/// when an LLM endpoint is configured, the <see cref="OpenAiCompatibleClient"/>. When
/// <c>Agent:LlmEndpoint</c> is empty the agent runs fully deterministically.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds the agent services to the container.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">App configuration (reads Agent section).</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAgent(this IServiceCollection services, IConfiguration configuration)
    {
        var catalog = ControlCatalog.Default();
        services.AddSingleton(catalog);

        var endpoint = configuration["Agent:LlmEndpoint"];
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            var model = configuration["Agent:Model"] ?? "local-model";
            services.AddHttpClient<OpenAiCompatibleClient>(client =>
            {
                client.BaseAddress = new Uri(endpoint);
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            // Resolve the LLM-backed agent via a factory so the client gets its base address.
            services.AddScoped<ComplianceAgent>(sp =>
            {
                var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OpenAiCompatibleClient));
                var llm = new OpenAiCompatibleClient(http, model);
                return new ComplianceAgent(llm, catalog);
            });
        }
        else
        {
            // No LLM configured: deterministic-only agent (offline safe).
            services.AddScoped<ComplianceAgent>(_ => new ComplianceAgent(null, catalog));
        }

        services.AddScoped<ComplianceAgentService>();
        return services;
    }
}
