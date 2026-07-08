using MediatR;
using MiniGrc.Application.DTOs;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Application.Queries;

/// <summary>Returns all controls, optionally filtered by framework.</summary>
public sealed record GetControlsQuery(ComplianceFramework? Framework = null) : IRequest<IReadOnlyList<ControlDto>>;

/// <summary>Returns a single control by id, or null.</summary>
public sealed record GetControlByIdQuery(Guid Id) : IRequest<ControlDto?>;

/// <summary>
/// Returns the aggregated compliance status used by the dashboard: coverage, status counts, and
/// a per-framework breakdown.
/// </summary>
public sealed record GetComplianceStatusQuery : IRequest<ComplianceStatusDto>;

/// <summary>Returns all findings, optionally only those the agent has not yet mapped.</summary>
public sealed record GetFindingsQuery(bool OnlyUnmapped = false) : IRequest<IReadOnlyList<FindingDto>>;

/// <summary>Returns all risk register entries.</summary>
public sealed record GetRisksQuery : IRequest<IReadOnlyList<RiskDto>>;
