using Mapster;
using MediatR;
using Augur.Application.Commands;
using Augur.Application.DTOs;
using Augur.Domain;
using Augur.Domain.Entities;

namespace Augur.Application.Handlers;

public sealed class CreateControlHandler : IRequestHandler<CreateControlCommand, ControlDto>
{
    private readonly IUnitOfWork _uow;

    public CreateControlHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ControlDto> Handle(CreateControlCommand request, CancellationToken cancellationToken)
    {
        var control = Control.Create(request.Code, request.Title, request.Description, request.Framework, request.Owner);
        await _uow.Controls.AddAsync(control, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return control.Adapt<ControlDto>();
    }
}

public sealed class UpdateControlHandler : IRequestHandler<UpdateControlCommand, ControlDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateControlHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ControlDto> Handle(UpdateControlCommand request, CancellationToken cancellationToken)
    {
        var control = await _uow.Controls.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw new KeyNotFoundException($"Control {request.Id} not found.");
        control.Update(request.Title, request.Description, request.Owner);
        await _uow.Controls.UpdateAsync(control, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return control.Adapt<ControlDto>();
    }
}

public sealed class AttachEvidenceHandler : IRequestHandler<AttachEvidenceCommand, EvidenceDto>
{
    private readonly IUnitOfWork _uow;

    public AttachEvidenceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EvidenceDto> Handle(AttachEvidenceCommand request, CancellationToken cancellationToken)
    {
        var control = await _uow.Controls.GetByIdAsync(request.ControlId, cancellationToken)
                      ?? throw new KeyNotFoundException($"Control {request.ControlId} not found.");
        var evidence = control.AttachEvidence(request.FileName, request.ContentType, request.SizeBytes, request.UploadedBy);
        await _uow.Controls.UpdateAsync(control, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return evidence.Adapt<EvidenceDto>();
    }
}

public sealed class ReviewEvidenceHandler : IRequestHandler<ReviewEvidenceCommand, ControlDto>
{
    private readonly IUnitOfWork _uow;

    public ReviewEvidenceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ControlDto> Handle(ReviewEvidenceCommand request, CancellationToken cancellationToken)
    {
        var control = await _uow.Controls.GetByIdAsync(request.ControlId, cancellationToken)
                      ?? throw new KeyNotFoundException($"Control {request.ControlId} not found.");
        control.ReviewEvidence(request.EvidenceId, request.Outcome, request.Reviewer);
        await _uow.Controls.UpdateAsync(control, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return control.Adapt<ControlDto>();
    }
}

public sealed class CreateRiskHandler : IRequestHandler<CreateRiskCommand, RiskDto>
{
    private readonly IUnitOfWork _uow;

    public CreateRiskHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<RiskDto> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = Risk.Create(request.Title, request.Description, request.Likelihood, request.Impact);
        await _uow.Risks.UpsertAsync(risk, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return risk.Adapt<RiskDto>();
    }
}
