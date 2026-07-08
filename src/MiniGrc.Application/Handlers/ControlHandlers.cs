using Mapster;
using MediatR;
using MiniGrc.Application.Commands;
using MiniGrc.Application.DTOs;
using MiniGrc.Domain;
using MiniGrc.Domain.Entities;

namespace MiniGrc.Application.Handlers;

/// <summary>Handles <see cref="CreateControlCommand"/> by building a <see cref="Control"/> and persisting it.</summary>
public sealed class CreateControlHandler : IRequestHandler<CreateControlCommand, ControlDto>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public CreateControlHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Creates and stores the control, then returns its read model.</summary>
    public async Task<ControlDto> Handle(CreateControlCommand request, CancellationToken cancellationToken)
    {
        var control = Control.Create(request.Code, request.Title, request.Description, request.Framework, request.Owner);
        await _uow.Controls.AddAsync(control, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return control.Adapt<ControlDto>();
    }
}

/// <summary>Handles <see cref="UpdateControlCommand"/>.</summary>
public sealed class UpdateControlHandler : IRequestHandler<UpdateControlCommand, ControlDto>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public UpdateControlHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Loads the control, applies the update, and returns its read model.</summary>
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

/// <summary>Handles <see cref="AttachEvidenceCommand"/>.</summary>
public sealed class AttachEvidenceHandler : IRequestHandler<AttachEvidenceCommand, EvidenceDto>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public AttachEvidenceHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Attaches evidence to the control and returns the evidence read model.</summary>
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

/// <summary>Handles <see cref="ReviewEvidenceCommand"/> and recomputes control status.</summary>
public sealed class ReviewEvidenceHandler : IRequestHandler<ReviewEvidenceCommand, ControlDto>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public ReviewEvidenceHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Applies the reviewer decision and returns the updated control read model.</summary>
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

/// <summary>Handles <see cref="CreateRiskCommand"/>.</summary>
public sealed class CreateRiskHandler : IRequestHandler<CreateRiskCommand, RiskDto>
{
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the handler with the unit of work port.</summary>
    public CreateRiskHandler(IUnitOfWork uow) => _uow = uow;

    /// <summary>Creates and stores a risk, returning its read model.</summary>
    public async Task<RiskDto> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = Risk.Create(request.Title, request.Description, request.Likelihood, request.Impact);
        await _uow.Risks.UpsertAsync(risk, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return risk.Adapt<RiskDto>();
    }
}
