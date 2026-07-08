using System.Text.Json;
using MiniGrc.Agent.Fallback;
using MiniGrc.Agent.Knowledge;
using MiniGrc.Agent.Llm;
using MiniGrc.Agent.Models;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Agent;

/// <summary>
/// The compliance agent orchestrator. It attempts to use an LLM for richer finding extraction and
/// a natural-language risk summary, but always degrades gracefully to the deterministic analyzer
/// when the model is unreachable (offline / no API key). This is the agent's core design: the LLM
/// is an enhancement, never a hard dependency — so the product works without one.
/// </summary>
public sealed class ComplianceAgent
{
    private readonly OpenAiCompatibleClient? _llm;
    private readonly ControlCatalog _catalog;
    private readonly DeterministicAnalyzer _fallback;

    /// <summary>Constructs the agent. Pass a null client to run fully deterministically.</summary>
    public ComplianceAgent(OpenAiCompatibleClient? llm, ControlCatalog catalog)
    {
        _llm = llm;
        _catalog = catalog;
        _fallback = new DeterministicAnalyzer(catalog);
    }

    /// <summary>The system prompt that frames the LLM as a GRC analyst.</summary>
    public const string SystemPrompt =
        "You are a compliance analyst for a GRC (governance, risk, compliance) platform. " +
        "You receive security findings from tools or policy documents and map each to the most " +
        "relevant control code from the provided catalog. Respond ONLY with strict JSON of the form " +
        "{\"findings\":[{\"title\",\"description\",\"severity\",\"external_id\",\"mapped_control_code\"," +
        "\"remediations\":[{\"title\",\"detail\",\"priority\"}]}],\"risk_summary\":\"...\"}.";

    /// <summary>
    /// Runs the agent over an input. Tries the LLM first; on any failure falls back to the
    /// deterministic analyzer so a result is always produced.
    /// </summary>
    /// <param name="request">The agent input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The aggregated agent result (findings + risk summary + whether the LLM was used).</returns>
    public async Task<AgentResult> RunAsync(AgentRequest request, CancellationToken ct = default)
    {
        var start = DateTime.UtcNow;
        List<ExtractedFinding> findings = new();
        string summary = string.Empty;
        var usedLlm = false;

        if (_llm is not null)
        {
            try
            {
                var catalogText = string.Join("\n", _catalog.Entries
                    .Where(e => e.Framework == request.Framework)
                    .Select(e => $"{e.Code}: {e.Title} (keywords: {string.Join(", ", e.Keywords)})"));
                var userPrompt =
                    $"Target framework: {request.Framework}.\n" +
                    $"Known controls:\n{catalogText}\n\n" +
                    $"Source: {request.Source} ({request.Format}).\n" +
                    $"Content:\n{request.Content}";

                var raw = await _llm.CompleteAsync(SystemPrompt, userPrompt, temperature: 0.2, ct);
                (findings, summary) = ParseLlm(raw, request);
                usedLlm = true;
            }
            catch (Exception ex)
            {
                // Failure mode: LLM unreachable or returned unusable JSON -> deterministic fallback.
                Console.WriteLine($"[Agent] LLM unavailable ({ex.GetType().Name}); using deterministic fallback.");
            }
        }

        if (!usedLlm)
        {
            findings = _fallback.Analyze(request.Source, request.Format, request.Content, request.Framework);
            summary = _fallback.Summarize(findings, request.Framework);
        }

        var elapsed = (long)(DateTime.UtcNow - start).TotalMilliseconds;
        return new AgentResult(findings, findings.Count(f => f.MappedControlCode is not null), summary, usedLlm, elapsed);
    }

    private static (List<ExtractedFinding>, string) ParseLlm(string raw, AgentRequest request)
    {
        // Strip markdown code fences if the model wrapped the JSON.
        var json = raw.Trim();
        if (json.StartsWith("```"))
            json = json.Substring(json.IndexOf('\n') + 1).TrimEnd('`', '\r', '\n');

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var findings = new List<ExtractedFinding>();
        if (root.TryGetProperty("findings", out var arr))
        {
            foreach (var el in arr.EnumerateArray())
            {
                var title = el.GetProperty("title").GetString() ?? "Untitled";
                var desc = el.TryGetProperty("description", out var d) ? d.GetString() : null;
                var sev = ParseSeverity(el.TryGetProperty("severity", out var s) ? s.GetString() : null);
                var ext = el.TryGetProperty("external_id", out var e) ? e.GetString() ?? $"{request.Source}-{findings.Count}"
                                                                       : $"{request.Source}-{findings.Count}";
                var code = el.TryGetProperty("mapped_control_code", out var c) ? c.GetString() : null;
                var rem = new List<RemediationSuggestion>();
                if (el.TryGetProperty("remediations", out var r))
                    foreach (var ri in r.EnumerateArray())
                    {
                        var rt = ri.GetProperty("title").GetString() ?? "Remediate";
                        var rd = ri.TryGetProperty("detail", out var rdEl) ? rdEl.GetString() : null;
                        var rp = ParsePriority(ri.TryGetProperty("priority", out var rpEl) ? rpEl.GetString() : null);
                        rem.Add(new RemediationSuggestion(rt, rd, rp));
                    }
                if (rem.Count == 0) rem.Add(new RemediationSuggestion($"Remediate '{title}'", null, RemediationPriority.Medium));
                findings.Add(new ExtractedFinding(title, desc, sev, ext, code, rem));
            }
        }
        var summary = root.TryGetProperty("risk_summary", out var sum) ? sum.GetString() ?? string.Empty : string.Empty;
        return (findings, summary);
    }

    private static FindingSeverity ParseSeverity(string? v) => v?.Trim().ToLowerInvariant() switch
    {
        "critical" => FindingSeverity.Critical,
        "high" => FindingSeverity.High,
        "medium" or "moderate" => FindingSeverity.Medium,
        "low" => FindingSeverity.Low,
        _ => FindingSeverity.Info
    };

    private static RemediationPriority ParsePriority(string? v) => v?.Trim().ToLowerInvariant() switch
    {
        "urgent" => RemediationPriority.Urgent,
        "high" => RemediationPriority.High,
        "medium" => RemediationPriority.Medium,
        "low" => RemediationPriority.Low,
        _ => RemediationPriority.Backlog
    };
}
