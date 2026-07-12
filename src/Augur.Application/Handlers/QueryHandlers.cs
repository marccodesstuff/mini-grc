using Mapster;
using MediatR;
using Augur.Application.DTOs;
using Augur.Application.Queries;
using Augur.Domain;
using Augur.Domain.Enums;

namespace Augur.Application.Handlers;

public sealed class GetControlsHandler : IRequestHandler<GetControlsQuery, IReadOnlyList<ControlDto>>
{
    private readonly IUnitOfWork _uow;

    public GetControlsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ControlDto>> Handle(GetControlsQuery request, CancellationToken cancellationToken)
    {
        var controls = await _uow.Controls.GetAllAsync(request.Framework, cancellationToken);
        return controls.Adapt<IReadOnlyList<ControlDto>>();
    }
}

public sealed class GetControlByIdHandler : IRequestHandler<GetControlByIdQuery, ControlDto?>
{
    private readonly IUnitOfWork _uow;

    public GetControlByIdHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ControlDto?> Handle(GetControlByIdQuery request, CancellationToken cancellationToken)
    {
        var control = await _uow.Controls.GetByIdAsync(request.Id, cancellationToken);
        return control?.Adapt<ControlDto>();
    }
}

public sealed class GetComplianceStatusHandler : IRequestHandler<GetComplianceStatusQuery, ComplianceStatusDto>
{
    private readonly IUnitOfWork _uow;

    public GetComplianceStatusHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ComplianceStatusDto> Handle(GetComplianceStatusQuery request, CancellationToken cancellationToken)
    {
        var controls = await _uow.Controls.GetAllAsync(framework: null, cancellationToken);
        var total = controls.Count;
        var verified = controls.Count(c => c.Status == ControlStatus.Verified);
        var partial = controls.Count(c => c.Status == ControlStatus.Partial);
        var notImplemented = controls.Count(c => c.Status == ControlStatus.NotImplemented);

        var byFramework = controls
            .GroupBy(c => c.Framework)
            .Select(g => new FrameworkBreakdownDto
            {
                Framework = g.Key.ToString(),
                Total = g.Count(),
                Verified = g.Count(c => c.Status == ControlStatus.Verified)
            })
            .ToList();

        return new ComplianceStatusDto
        {
            TotalControls = total,
            VerifiedControls = verified,
            PartialControls = partial,
            NotImplementedControls = notImplemented,
            CoveragePercent = total == 0 ? 0 : Math.Round(verified * 100.0 / total, 1),
            ByFramework = byFramework
        };
    }
}

public sealed class GetFindingsHandler : IRequestHandler<GetFindingsQuery, IReadOnlyList<FindingDto>>
{
    private readonly IUnitOfWork _uow;

    public GetFindingsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<FindingDto>> Handle(GetFindingsQuery request, CancellationToken cancellationToken)
    {
        var findings = await _uow.Findings.GetAllAsync(request.OnlyUnmapped, cancellationToken);
        return findings.Adapt<IReadOnlyList<FindingDto>>();
    }
}

public sealed class GetRisksHandler : IRequestHandler<GetRisksQuery, IReadOnlyList<RiskDto>>
{
    private readonly IUnitOfWork _uow;

    public GetRisksHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<RiskDto>> Handle(GetRisksQuery request, CancellationToken cancellationToken)
    {
        var risks = await _uow.Risks.GetAllAsync(cancellationToken);
        return risks.Adapt<IReadOnlyList<RiskDto>>();
    }
}
