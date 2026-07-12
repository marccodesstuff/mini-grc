using MediatR;
using Augur.Application.DTOs;
using Augur.Domain.Enums;

namespace Augur.Application.Queries;

public sealed record GetControlsQuery(ComplianceFramework? Framework = null) : IRequest<IReadOnlyList<ControlDto>>;

public sealed record GetControlByIdQuery(Guid Id) : IRequest<ControlDto?>;

public sealed record GetComplianceStatusQuery : IRequest<ComplianceStatusDto>;

public sealed record GetFindingsQuery(bool OnlyUnmapped = false) : IRequest<IReadOnlyList<FindingDto>>;

public sealed record GetRisksQuery : IRequest<IReadOnlyList<RiskDto>>;
