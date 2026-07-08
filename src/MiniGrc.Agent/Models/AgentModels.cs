using MiniGrc.Domain.Enums;

namespace MiniGrc.Agent.Models;

/// <summary>Input to the compliance agent: a policy document or a security-tool export.</summary>
public sealed record AgentRequest(
    /// <summary>Source system name (e.g. "github-dependabot", "policy-doc").</summary>
    string Source,
    /// <summary>Input format: "json" (tool export) or "text" (policy prose).</summary>
    string Format,
    /// <summary>Raw payload content.</summary>
    string Content,
    /// <summary>Target framework to map against ("Soc2" or "Iso27001").</summary>
    ComplianceFramework Framework);

/// <summary>A single finding the agent extracted and mapped.</summary>
public sealed record ExtractedFinding(
    string Title,
    string? Description,
    FindingSeverity Severity,
    string ExternalId,
    string? MappedControlCode,
    IReadOnlyList<RemediationSuggestion> Remediations);

/// <summary>A remediation task the agent suggests for a finding.</summary>
public sealed record RemediationSuggestion(string Title, string? Detail, RemediationPriority Priority);

/// <summary>Aggregated output of a single agent run.</summary>
public sealed record AgentResult(
    /// <summary>Findings the agent created and persisted.</summary>
    IReadOnlyList<ExtractedFinding> Findings,
    /// <summary>Count of findings successfully mapped to an existing/suggested control.</summary>
    int MappedCount,
    /// <summary>Short natural-language risk summary written by the agent.</summary>
    string RiskSummary,
    /// <summary>Whether the run used the LLM (true) or the deterministic fallback (false).</summary>
    bool UsedLlm,
    /// <summary>Timing of the run in milliseconds.</summary>
    long ElapsedMs);
