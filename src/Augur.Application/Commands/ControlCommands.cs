using MediatR;
using Augur.Application.DTOs;
using Augur.Domain.Enums;

namespace Augur.Application.Commands;

public sealed record CreateControlCommand(
    string Code,
    string Title,
    string Description,
    ComplianceFramework Framework,
    string Owner) : IRequest<ControlDto>;

public sealed record UpdateControlCommand(
    Guid Id,
    string Title,
    string Description,
    string Owner) : IRequest<ControlDto>;

public sealed record AttachEvidenceCommand(
    Guid ControlId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string UploadedBy) : IRequest<EvidenceDto>;

public sealed record ReviewEvidenceCommand(
    Guid ControlId,
    Guid EvidenceId,
    EvidenceStatus Outcome,
    string? Reviewer) : IRequest<ControlDto>;

public sealed record CreateRiskCommand(
    string Title,
    string? Description,
    int Likelihood,
    int Impact) : IRequest<RiskDto>;
