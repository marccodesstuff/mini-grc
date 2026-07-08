using Mapster;
using MediatR;
using MiniGrc.Application.DTOs;
using MiniGrc.Application.Queries;
using MiniGrc.Domain;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Application.Handlers;

/// <summary>Handles <see cref="GetControlsQuery"/>.</summary>
public sealed class GetControlsHandler : IRequestHandler<GetControlsQuery, IReadOnlyList<ControlDto>>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public GetControlsHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Returns controls (optionally filtered by framework) as read models.</summary>
    public async Task<IReadOnlyList<ControlDto>> Handle(GetControlsQuery request, CancellationToken cancellationToken)
    {
        var controls = await _uow.Controls.GetAllAsync(request.Framework, cancellationToken);
        return controls.Adapt<IReadOnlyList<ControlDto>>();
    }
}

/// <summary>Handles <see cref="GetControlByIdQuery"/>.</summary>
public sealed class GetControlByIdHandler : IRequestHandler<GetControlByIdQuery, ControlDto?>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public GetControlByIdHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Returns a single control read model, or null when not found.</summary>
    public async Task<ControlDto?> Handle(GetControlByIdQuery request, CancellationToken cancellationToken)
    {
        var control = await _uow.Controls.GetByIdAsync(request.Id, cancellationToken);
        return control?.Adapt<ControlDto>();
    }
}

/// <summary>Handles <see cref="GetComplianceStatusQuery"/> by aggregating control status.</summary>
public sealed class GetComplianceStatusHandler : IRequestHandler<GetComplianceStatusQuery, ComplianceStatusDto>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public GetComplianceStatusHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Computes coverage, status counts, and a per-framework breakdown.</summary>
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

/// <summary>Handles <see cref="GetFindingsQuery"/>.</summary>
public sealed class GetFindingsHandler : IRequestHandler<GetFindingsQuery, IReadOnlyList<FindingDto>>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public GetFindingsHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Returns findings (optionally only unmapped) as read models.</summary>
    public async Task<IReadOnlyList<FindingDto>> Handle(GetFindingsQuery request, CancellationToken cancellationToken)
    {
        var findings = await _uow.Findings.GetAllAsync(request.OnlyUnmapped, cancellationToken);
        return findings.Adapt<IReadOnlyList<FindingDto>>();
    }
}

/// <summary>Handles <see cref="GetRisksQuery"/>.</summary>
public sealed class GetRisksHandler : IRequestHandler<GetRisksQuery, IReadOnlyList<RiskDto>>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public GetRisksHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Returns all risk register entries as read models.</summary>
    public async Task<IReadOnlyList<RiskDto>> Handle(GetRisksQuery request, CancellationToken cancellationToken)
    {
        var risks = await _uow.Risks.GetAllAsync(cancellationToken);
        return risks.Adapt<IReadOnlyList<RiskDto>>();
    }
}
