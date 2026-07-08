using MediatR;
using MiniGrc.Application.DTOs;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Application.Commands;

/// <summary>Creates a new security/compliance control.</summary>
public sealed record CreateControlCommand(
    string Code,
    string Title,
    string Description,
    ComplianceFramework Framework,
    string Owner) : IRequest<ControlDto>;

/// <summary>Updates mutable metadata on an existing control.</summary>
public sealed record UpdateControlCommand(
    Guid Id,
    string Title,
    string Description,
    string Owner) : IRequest<ControlDto>;

/// <summary>Attaches an evidence artifact (by metadata) to a control and recomputes its status.</summary>
public sealed record AttachEvidenceCommand(
    Guid ControlId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string UploadedBy) : IRequest<EvidenceDto>;

/// <summary>Records a reviewer decision on an evidence artifact and recomputes control status.</summary>
public sealed record ReviewEvidenceCommand(
    Guid ControlId,
    Guid EvidenceId,
    EvidenceStatus Outcome,
    string? Reviewer) : IRequest<ControlDto>;

/// <summary>Registers a new risk register entry.</summary>
public sealed record CreateRiskCommand(
    string Title,
    string? Description,
    int Likelihood,
    int Impact) : IRequest<RiskDto>;
