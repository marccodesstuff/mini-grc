namespace MiniGrc.Domain.Enums;

/// <summary>Compliance framework a control belongs to. Drives how controls are grouped and reported.</summary>
public enum ComplianceFramework
{
    /// <summary>Trust Services Criteria (AICPA SOC 2).</summary>
    Soc2 = 0,

    /// <summary>ISO/IEC 27001:2022 Annex A controls.</summary>
    Iso27001 = 1
}

/// <summary>Lifecycle status of a security control.</summary>
public enum ControlStatus
{
    /// <summary>Control has not been implemented yet.</summary>
    NotImplemented = 0,

    /// <summary>Partially implemented; some evidence collected.</summary>
    Partial = 1,

    /// <summary>Fully implemented but not yet independently verified.</summary>
    Implemented = 2,

    /// <summary>Implemented and verified with approved evidence.</summary>
    Verified = 3
}

/// <summary>Review state of an uploaded evidence artifact.</summary>
public enum EvidenceStatus
{
    /// <summary>Awaiting reviewer sign-off.</summary>
    PendingReview = 0,

    /// <summary>Accepted as proof the control operates.</summary>
    Approved = 1,

    /// <summary>Rejected; does not satisfy the control.</summary>
    Rejected = 2
}

/// <summary>Severity of a security finding surfaced by a tool or policy review.</summary>
public enum FindingSeverity
{
    Info = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>Residual risk rating derived from likelihood x impact.</summary>
public enum RiskSeverity
{
    Negligible = 0,
    Low = 1,
    Moderate = 2,
    High = 3,
    Extreme = 4
}

/// <summary>Work priority of a remediation task.</summary>
public enum RemediationPriority
{
    Backlog = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}
