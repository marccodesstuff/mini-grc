using System.Net.Http.Json;
using System.Text.Json;
using MiniGrc.Application.DTOs;

namespace MiniGrc.Web.Services;

/// <summary>
/// Typed HTTP client for the Mini-GRC API. The Blazor front end is a separate deployment from the
/// API; this client is the only place that knows the base URL and the wire format. It reuses the
/// Application-layer DTOs so the read models are shared across the stack.
/// </summary>
public sealed class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Constructs the client with a pre-configured HttpClient.</summary>
    public ApiClient(HttpClient http) => _http = http;

    /// <summary>Returns all controls (optionally filtered by framework).</summary>
    public async Task<IReadOnlyList<ControlDto>> GetControlsAsync(string? framework = null)
    {
        var url = framework is null ? "api/v1/controls" : $"api/v1/controls?framework={Uri.EscapeDataString(framework)}";
        return await _http.GetFromJsonAsync<IReadOnlyList<ControlDto>>(url, JsonOptions) ?? Array.Empty<ControlDto>();
    }

    /// <summary>Creates a control.</summary>
    public async Task<ControlDto> CreateControlAsync(object payload)
        => (await _http.PostAsJsonAsync("api/v1/controls", payload, JsonOptions)).EnsureSuccessStatusCode()
           .Content.ReadFromJsonAsync<ControlDto>(JsonOptions).Result!;

    /// <summary>Attaches evidence metadata to a control.</summary>
    public async Task<EvidenceDto> AttachEvidenceAsync(Guid controlId, object payload)
        => (await _http.PostAsJsonAsync($"api/v1/controls/{controlId}/evidence", payload, JsonOptions)).EnsureSuccessStatusCode()
           .Content.ReadFromJsonAsync<EvidenceDto>(JsonOptions).Result!;

    /// <summary>Returns the aggregated compliance status.</summary>
    public async Task<ComplianceStatusDto> GetComplianceStatusAsync()
        => await _http.GetFromJsonAsync<ComplianceStatusDto>("api/v1/compliance/status", JsonOptions)
           ?? new ComplianceStatusDto();

    /// <summary>Returns all findings.</summary>
    public async Task<IReadOnlyList<FindingDto>> GetFindingsAsync()
        => await _http.GetFromJsonAsync<IReadOnlyList<FindingDto>>("api/v1/findings", JsonOptions)
           ?? Array.Empty<FindingDto>();

    /// <summary>Runs the compliance agent and returns the result.</summary>
    public async Task<AgentResult> RunAgentAsync(object payload)
        => (await _http.PostAsJsonAsync("api/v1/agent/run", payload, JsonOptions)).EnsureSuccessStatusCode()
           .Content.ReadFromJsonAsync<AgentResult>(JsonOptions).Result!;
}

/// <summary>Lightweight shape of the agent run result returned by the API (mirrors the DTO).</summary>
public sealed record AgentResult(
    IReadOnlyList<AgentFinding> Findings,
    int MappedCount,
    string RiskSummary,
    bool UsedLlm,
    long ElapsedMs);

/// <summary>Lightweight shape of an agent finding.</summary>
public sealed record AgentFinding(
    string Title,
    string? Description,
    int Severity,
    string ExternalId,
    string? MappedControlCode,
    IReadOnlyList<AgentRemediation> Remediations);

/// <summary>Lightweight shape of an agent remediation suggestion.</summary>
public sealed record AgentRemediation(string Title, string? Detail, int Priority);
