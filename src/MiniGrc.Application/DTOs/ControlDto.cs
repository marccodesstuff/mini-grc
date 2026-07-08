using MiniGrc.Domain.Entities;

namespace MiniGrc.Application.DTOs;

/// <summary>Read model for a <see cref="Control"/>, including a flattened evidence summary.</summary>
public sealed class ControlDto
{
    /// <summary>Control id.</summary>
    public Guid Id { get; init; }

    /// <summary>Control code (e.g. "SOC2-CC6.1").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Control title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Control description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Compliance framework.</summary>
    public string Framework { get; init; } = string.Empty;

    /// <summary>Current status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Control owner.</summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>Number of evidence artifacts attached.</summary>
    public int EvidenceCount { get; init; }

    /// <summary>Number of approved evidence artifacts.</summary>
    public int ApprovedEvidenceCount { get; init; }

    /// <summary>UTC updated timestamp.</summary>
    public DateTime UpdatedAtUtc { get; init; }
}

/// <summary>Read model for an <see cref="Evidence"/> artifact.</summary>
public sealed class EvidenceDto
{
    /// <summary>Evidence id.</summary>
    public Guid Id { get; init; }

    /// <summary>File name.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Content type.</summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Size in bytes.</summary>
    public long SizeBytes { get; init; }

    /// <summary>Uploader.</summary>
    public string UploadedBy { get; init; } = string.Empty;

    /// <summary>Review status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Reviewer, if reviewed.</summary>
    public string? Reviewer { get; init; }
}

/// <summary>Read model for a <see cref="Finding"/> including its remediation tasks.</summary>
public sealed class FindingDto
{
    /// <summary>Finding id.</summary>
    public Guid Id { get; init; }

    /// <summary>Title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Severity.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Source system.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>External id from the source system.</summary>
    public string ExternalId { get; init; } = string.Empty;

    /// <summary>Whether the agent mapped it to a control.</summary>
    public bool Mapped { get; init; }

    /// <summary>Control id the agent mapped to, if any.</summary>
    public Guid? MappedControlId { get; init; }

    /// <summary>Suggested control code, if any.</summary>
    public string? SuggestedControlCode { get; init; }

    /// <summary>Remediation tasks drafted by the agent.</summary>
    public IReadOnlyList<RemediationTaskDto> RemediationTasks { get; init; } = Array.Empty<RemediationTaskDto>();
}

/// <summary>Read model for a remediation task.</summary>
public sealed class RemediationTaskDto
{
    /// <summary>Task id.</summary>
    public Guid Id { get; init; }

    /// <summary>Title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Detail.</summary>
    public string? Detail { get; init; }

    /// <summary>Priority.</summary>
    public string Priority { get; init; } = string.Empty;
}

/// <summary>Read model for a <see cref="Risk"/> register entry.</summary>
public sealed class RiskDto
{
    /// <summary>Risk id.</summary>
    public Guid Id { get; init; }

    /// <summary>Title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Likelihood 1-5.</summary>
    public int Likelihood { get; init; }

    /// <summary>Impact 1-5.</summary>
    public int Impact { get; init; }

    /// <summary>Derived severity.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Whether accepted.</summary>
    public bool Accepted { get; init; }
}

/// <summary>Aggregated compliance status for the dashboard.</summary>
public sealed class ComplianceStatusDto
{
    /// <summary>Total number of controls.</summary>
    public int TotalControls { get; init; }

    /// <summary>Controls with status Verified.</summary>
    public int VerifiedControls { get; init; }

    /// <summary>Controls partially evidenced.</summary>
    public int PartialControls { get; init; }

    /// <summary>Controls not yet implemented.</summary>
    public int NotImplementedControls { get; init; }

    /// <summary>Compliance coverage percentage (Verified / Total).</summary>
    public double CoveragePercent { get; init; }

    /// <summary>Counts broken down by framework.</summary>
    public IReadOnlyList<FrameworkBreakdownDto> ByFramework { get; init; } = Array.Empty<FrameworkBreakdownDto>();
}

/// <summary>Per-framework rollup used inside <see cref="ComplianceStatusDto"/>.</summary>
public sealed class FrameworkBreakdownDto
{
    /// <summary>Framework name.</summary>
    public string Framework { get; init; } = string.Empty;

    /// <summary>Total controls in the framework.</summary>
    public int Total { get; init; }

    /// <summary>Verified controls in the framework.</summary>
    public int Verified { get; init; }
}
