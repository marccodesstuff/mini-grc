using MiniGrc.Domain.Enums;

namespace MiniGrc.Agent.Knowledge;

/// <summary>
/// Static catalog of canonical controls per framework. The agent uses this to map a finding's
/// keywords to the most relevant control code. In production this would be loaded from the
/// database; a static catalog keeps the mapping explainable and testable.
/// </summary>
public sealed class ControlCatalog
{
    /// <summary>A catalog entry linking a control code to the keywords that suggest it.</summary>
    public sealed record Entry(string Code, string Title, ComplianceFramework Framework, string[] Keywords);

    /// <summary>All known catalog entries.</summary>
    public IReadOnlyList<Entry> Entries { get; }

    /// <summary>Builds the catalog from the supplied entries.</summary>
    public ControlCatalog(IEnumerable<Entry> entries) => Entries = entries.ToList();

    /// <summary>Default SOC 2 + ISO 27001 catalog used by the agent.</summary>
    public static ControlCatalog Default() => new(new[]
    {
        new Entry("SOC2-CC6.1", "Logical Access Controls", ComplianceFramework.Soc2,
            new[] { "access", "authentication", "mfa", "login", "password", "credential", "iam", "rbac", "permission" }),
        new Entry("SOC2-CC6.6", "Boundary Protection / Firewall", ComplianceFramework.Soc2,
            new[] { "firewall", "network", "port", "exposure", "ingress", "vpc", "segmentation" }),
        new Entry("SOC2-CC7.1", "Vulnerability Detection", ComplianceFramework.Soc2,
            new[] { "vulnerability", "cve", "dependabot", "sca", "patch", "outdated", "dependency" }),
        new Entry("SOC2-CC7.2", "Monitoring and Anomaly Detection", ComplianceFramework.Soc2,
            new[] { "monitor", "logging", "audit", "alert", "anomaly", "intrusion", "ids" }),
        new Entry("SOC2-CC6.8", "Defect Prevention / Malware", ComplianceFramework.Soc2,
            new[] { "malware", "virus", "ransomware", "antivirus", "exploit" }),
        new Entry("ISO-A.8.5", "Secure Authentication", ComplianceFramework.Iso27001,
            new[] { "authentication", "mfa", "password", "credential", "access" }),
        new Entry("ISO-A.8.8", "Management of Technical Vulnerabilities", ComplianceFramework.Iso27001,
            new[] { "vulnerability", "cve", "patch", "dependency", "dependabot", "sca" }),
        new Entry("ISO-A.8.20", "Network Controls", ComplianceFramework.Iso27001,
            new[] { "firewall", "network", "port", "exposure", "segmentation" }),
        new Entry("ISO-A.8.15", "Logging", ComplianceFramework.Iso27001,
            new[] { "logging", "audit", "monitor", "log" }),
        new Entry("ISO-A.5.7", "Threat Intelligence", ComplianceFramework.Iso27001,
            new[] { "threat", "intelligence", "malware", "exploit" })
    });

    /// <summary>Maps a finding title/description to the best-matching control code, or null.</summary>
    public string? MapToControlCode(string text, ComplianceFramework framework)
    {
        var haystack = text.ToLowerInvariant();
        var best = Entries
            .Where(e => e.Framework == framework)
            .Select(e => new { e, Score = e.Keywords.Count(k => haystack.Contains(k)) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();
        return best?.e.Code;
    }
}
