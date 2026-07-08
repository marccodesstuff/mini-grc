using MiniGrc.Domain.Common;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Domain.Entities;

/// <summary>
/// A security finding surfaced by a tool (e.g. GitHub Dependabot, a vulnerability scanner) or
/// by a policy review. The AI agent maps findings onto <see cref="Control"/>s and drafts
/// remediation tasks. Findings are produced by the agent pipeline and stored for audit.
/// </summary>
public sealed class Finding : Entity, IAggregateRoot
{
    /// <summary>Short title of the finding.</summary>
    public string Title { get; private set; }

    /// <summary>Detailed description of the issue.</summary>
    public string? Description { get; private set; }

    /// <summary>Severity assigned by the originating tool or the agent.</summary>
    public FindingSeverity Severity { get; private set; }

    /// <summary>Source system that emitted the finding (e.g. "github-dependabot").</summary>
    public string Source { get; private set; }

    /// <summary>Identifier from the source system, used for de-duplication.</summary>
    public string ExternalId { get; private set; }

    /// <summary>Control this finding was mapped to by the agent, if any.</summary>
    public Guid? MappedControlId { get; private set; }

    /// <summary>Control code the agent suggested (may not yet exist as a control).</summary>
    public string? SuggestedControlCode { get; private set; }

    /// <summary>Whether the agent successfully mapped the finding to a control.</summary>
    public bool Mapped { get; private set; }

    /// <summary>Remediation tasks the agent drafted for this finding.</summary>
    public IReadOnlyList<RemediationTask> RemediationTasks => _remediationTasks.AsReadOnly();

    private readonly List<RemediationTask> _remediationTasks = new();

    private Finding()
    {
        Title = string.Empty;
        Source = string.Empty;
        ExternalId = string.Empty;
    }

    private Finding(string title, string? description, FindingSeverity severity, string source, string externalId)
    {
        Title = title;
        Description = description;
        Severity = severity;
        Source = source;
        ExternalId = externalId;
    }

    /// <summary>Factory that records a raw finding before agent mapping.</summary>
    public static Finding Create(string title, string? description, FindingSeverity severity, string source, string externalId)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Finding title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Finding source is required.", nameof(source));

        return new Finding(title.Trim(), description?.Trim(), severity, source.Trim(), externalId.Trim());
    }

    /// <summary>Records the agent's mapping decision for this finding.</summary>
    public void ApplyMapping(Guid? mappedControlId, string? suggestedControlCode)
    {
        MappedControlId = mappedControlId;
        SuggestedControlCode = suggestedControlCode?.Trim();
        Mapped = mappedControlId.HasValue || !string.IsNullOrWhiteSpace(suggestedControlCode);
        Touch();
    }

    /// <summary>Adds a remediation task drafted by the agent.</summary>
    public RemediationTask AddRemediationTask(string title, string? detail, RemediationPriority priority)
    {
        var task = RemediationTask.Create(title, detail, priority, Id);
        _remediationTasks.Add(task);
        Touch();
        return task;
    }
}
