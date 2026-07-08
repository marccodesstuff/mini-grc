using MiniGrc.Domain.Enums;

namespace MiniGrc.Api.Requests;

/// <summary>Payload for creating a control.</summary>
public sealed record CreateControlRequest(
    string Code,
    string Title,
    string Description,
    ComplianceFramework Framework,
    string Owner);

/// <summary>Payload for updating a control.</summary>
public sealed record UpdateControlRequest(string Title, string Description, string Owner);

/// <summary>Payload for attaching evidence metadata.</summary>
public sealed record AttachEvidenceRequest(string FileName, string ContentType, long SizeBytes, string UploadedBy);

/// <summary>Payload for a reviewer decision.</summary>
public sealed record ReviewEvidenceRequest(EvidenceStatus Outcome, string? Reviewer);

/// <summary>Payload for registering a risk.</summary>
public sealed record CreateRiskRequest(string Title, string? Description, int Likelihood, int Impact);

/// <summary>Payload for running the compliance agent.</summary>
public sealed record RunAgentRequest(
    string Source,
    string Format,
    string Content,
    string Framework);
