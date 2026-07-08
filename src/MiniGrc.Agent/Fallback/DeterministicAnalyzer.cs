using System.Text.Json;
using MiniGrc.Agent.Models;
using MiniGrc.Domain.Enums;
using MiniGrc.Agent.Knowledge;

namespace MiniGrc.Agent.Fallback;

/// <summary>
/// Deterministic, offline-safe brain for the agent. Used when no LLM endpoint is reachable, so
/// the app always produces a useful, inspectable result. Parses a tool JSON export or policy
/// prose, maps findings to controls via <see cref="ControlCatalog"/>, drafts remediation tasks,
/// and writes a plain risk summary.
/// </summary>
public sealed class DeterministicAnalyzer
{
    private readonly ControlCatalog _catalog;

    /// <summary>Constructs the analyzer with a control catalog.</summary>
    public DeterministicAnalyzer(ControlCatalog catalog) => _catalog = catalog;

    /// <summary>Analyzes raw input and returns extracted findings (no persistence).</summary>
    public List<ExtractedFinding> Analyze(string source, string format, string content, ComplianceFramework framework)
    {
        var raw = format.Equals("json", StringComparison.OrdinalIgnoreCase)
            ? ParseToolExport(source, content)
            : ParsePolicyProse(content);

        var findings = new List<ExtractedFinding>();
        foreach (var item in raw)
        {
            var code = _catalog.MapToControlCode($"{item.Title} {item.Description}", framework);
            var severity = item.Severity;
            var priority = severity switch
            {
                FindingSeverity.Critical => RemediationPriority.Urgent,
                FindingSeverity.High => RemediationPriority.High,
                FindingSeverity.Medium => RemediationPriority.Medium,
                FindingSeverity.Low => RemediationPriority.Low,
                _ => RemediationPriority.Backlog
            };

            var remediations = new List<RemediationSuggestion>
            {
                new($"Remediate '{item.Title}'", DraftDetail(item, code), priority)
            };
            if (code is not null)
                remediations.Add(new($"Update evidence for {code}", $"Attach proof that {code} now covers this finding.", RemediationPriority.Medium));

            findings.Add(new ExtractedFinding(item.Title, item.Description, severity, item.ExternalId, code, remediations));
        }

        return findings;
    }

    /// <summary>Writes a short natural-language risk summary from the extracted findings.</summary>
    public string Summarize(IReadOnlyList<ExtractedFinding> findings, ComplianceFramework framework)
    {
        if (findings.Count == 0)
            return $"No findings were detected in the {framework} scope. The control environment appears clean for this input.";

        var bySev = findings.GroupBy(f => f.Severity).OrderByDescending(g => g.Key).ToList();
        var lines = bySev.Select(g => $"{g.Count()} {g.Key} severit(y/ies)");
        var high = findings.Count(f => f.Severity >= FindingSeverity.High);
        var mapped = findings.Count(f => f.MappedControlCode is not null);
        return $"Analyzed {findings.Count} finding(s) against {framework}. " +
               $"Severity breakdown: {string.Join(", ", lines)}. " +
               $"{high} are high/critical and need immediate attention. " +
               $"{mapped} of {findings.Count} were mapped to a control code.";
    }

    private static string DraftDetail(RawFinding item, string? code) =>
        $"Investigate '{item.Title}' (source severity {item.Severity})." +
        (code is not null ? $" Ensure {code} is evidenced as remediated." : " No direct control mapping; assign an owner.");

    private List<RawFinding> ParseToolExport(string source, string content)
    {
        var findings = new List<RawFinding>();
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var array = root.ValueKind == JsonValueKind.Array ? root : root.GetProperty("findings");
            foreach (var el in array.EnumerateArray())
            {
                var title = el.GetProperty("title").GetString() ?? el.GetProperty("name").GetString() ?? "Untitled finding";
                var desc = el.TryGetProperty("description", out var d) ? d.GetString() : null;
                var sev = ParseSeverity(el.TryGetProperty("severity", out var s) ? s.GetString() : null);
                var ext = el.TryGetProperty("id", out var id) ? id.GetString() ?? $"{source}-{findings.Count}"
                          : el.TryGetProperty("external_id", out var eid) ? eid.GetString() ?? $"{source}-{findings.Count}"
                          : $"{source}-{findings.Count}";
                findings.Add(new RawFinding(title, desc, sev, ext));
            }
        }
        catch (JsonException)
        {
            // If the JSON is malformed, fall back to treating the whole blob as one finding.
            findings.Add(new RawFinding($"{source} export", content.Length > 200 ? content[..200] : content, FindingSeverity.Medium, $"{source}-raw"));
        }

        if (findings.Count == 0)
            findings.Add(new RawFinding($"{source} export", "No structured findings parsed.", FindingSeverity.Low, $"{source}-empty"));
        return findings;
    }

    private List<RawFinding> ParsePolicyProse(string content)
    {
        // Split prose into sentence-ish chunks and turn each into a finding candidate.
        var sentences = content.Split(['.', '\n', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 12)
            .Take(12);
        var findings = new List<RawFinding>();
        var i = 0;
        foreach (var s in sentences)
        {
            var sev = s.Contains("must", StringComparison.OrdinalIgnoreCase) || s.Contains("shall", StringComparison.OrdinalIgnoreCase)
                ? FindingSeverity.Medium : FindingSeverity.Low;
            findings.Add(new RawFinding($"Policy clause {++i}", s, sev, $"policy-{i}"));
        }
        return findings;
    }

    private static FindingSeverity ParseSeverity(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "critical" => FindingSeverity.Critical,
        "high" => FindingSeverity.High,
        "medium" or "moderate" => FindingSeverity.Medium,
        "low" => FindingSeverity.Low,
        "info" or "informational" => FindingSeverity.Info,
        _ => FindingSeverity.Medium
    };

    private sealed record RawFinding(string Title, string? Description, FindingSeverity Severity, string ExternalId);
}
