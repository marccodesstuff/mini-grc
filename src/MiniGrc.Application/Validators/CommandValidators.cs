using FluentValidation;
using MiniGrc.Application.Commands;

namespace MiniGrc.Application.Validators;

/// <summary>Validates <see cref="CreateControlCommand"/>. Fails fast before the handler runs.</summary>
public sealed class CreateControlCommandValidator : AbstractValidator<CreateControlCommand>
{
    /// <summary>Configures the rules.</summary>
    public CreateControlCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Owner).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Validates <see cref="UpdateControlCommand"/>.</summary>
public sealed class UpdateControlCommandValidator : AbstractValidator<UpdateControlCommand>
{
    /// <summary>Configures the rules.</summary>
    public UpdateControlCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Owner).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Validates <see cref="AttachEvidenceCommand"/>.</summary>
public sealed class AttachEvidenceCommandValidator : AbstractValidator<AttachEvidenceCommand>
{
    /// <summary>Configures the rules.</summary>
    public AttachEvidenceCommandValidator()
    {
        RuleFor(x => x.ControlId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.SizeBytes).GreaterThan(0);
        RuleFor(x => x.UploadedBy).NotEmpty();
    }
}

/// <summary>Validates <see cref="ReviewEvidenceCommand"/>.</summary>
public sealed class ReviewEvidenceCommandValidator : AbstractValidator<ReviewEvidenceCommand>
{
    /// <summary>Configures the rules.</summary>
    public ReviewEvidenceCommandValidator()
    {
        RuleFor(x => x.ControlId).NotEmpty();
        RuleFor(x => x.EvidenceId).NotEmpty();
    }
}

/// <summary>Validates <see cref="CreateRiskCommand"/>.</summary>
public sealed class CreateRiskCommandValidator : AbstractValidator<CreateRiskCommand>
{
    /// <summary>Configures the rules.</summary>
    public CreateRiskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Likelihood).InclusiveBetween(1, 5);
        RuleFor(x => x.Impact).InclusiveBetween(1, 5);
    }
}
