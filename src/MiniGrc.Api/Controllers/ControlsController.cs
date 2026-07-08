using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniGrc.Api.Requests;
using MiniGrc.Application.Commands;
using MiniGrc.Application.DTOs;
using MiniGrc.Application.Queries;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Api.Controllers;

/// <summary>
/// REST surface for security/compliance controls, their evidence, and the
/// compliance status dashboard. All endpoints are documented with XML comments and exported to
/// the OpenAPI 3.1.1 document at <c>/openapi/v1.json</c>.
/// </summary>
[ApiController]
[Route("api/v1/controls")]
[Produces("application/json")]
public sealed class ControlsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Constructs the controller with the MediatR mediator.</summary>
    public ControlsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all controls, optionally filtered by compliance framework.</summary>
    /// <param name="framework">Optional framework filter ("Soc2" or "Iso27001").</param>
    /// <returns>The collection of controls.</returns>
    /// <response code="200">Returns the controls.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ControlDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ControlDto>>> GetAll([FromQuery] string? framework)
    {
        ComplianceFramework? filter = ParseFramework(framework);
        var result = await _mediator.Send(new GetControlsQuery(filter));
        return Ok(result);
    }

    /// <summary>Gets a single control by its id.</summary>
    /// <param name="id">The control id.</param>
    /// <returns>The control, or 404 when not found.</returns>
    /// <response code="200">Control found.</response>
    /// <response code="404">Control not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ControlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ControlDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetControlByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new control.</summary>
    /// <param name="request">The control attributes.</param>
    /// <returns>The created control with its generated id.</returns>
    /// <response code="201">Control created.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ControlDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ControlDto>> Create([FromBody] CreateControlRequest request)
    {
        var command = new CreateControlCommand(
            request.Code, request.Title, request.Description, request.Framework, request.Owner);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates mutable metadata on a control.</summary>
    /// <param name="id">The control id.</param>
    /// <param name="request">The updated fields.</param>
    /// <returns>The updated control, or 404.</returns>
    /// <response code="200">Control updated.</response>
    /// <response code="404">Control not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ControlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ControlDto>> Update(Guid id, [FromBody] UpdateControlRequest request)
    {
        var result = await _mediator.Send(new UpdateControlCommand(id, request.Title, request.Description, request.Owner));
        return Ok(result);
    }

    /// <summary>Attaches an evidence artifact (metadata only) to a control.</summary>
    /// <param name="id">The control id.</param>
    /// <param name="request">The evidence metadata.</param>
    /// <returns>The created evidence record.</returns>
    /// <response code="200">Evidence attached.</response>
    /// <response code="404">Control not found.</response>
    [HttpPost("{id:guid}/evidence")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> AttachEvidence(Guid id, [FromBody] AttachEvidenceRequest request)
    {
        var result = await _mediator.Send(new AttachEvidenceCommand(
            id, request.FileName, request.ContentType, request.SizeBytes, request.UploadedBy));
        return Ok(result);
    }

    /// <summary>Records a reviewer decision on a control's evidence artifact.</summary>
    /// <param name="id">The control id.</param>
    /// <param name="evidenceId">The evidence id.</param>
    /// <param name="request">The outcome and optional reviewer.</param>
    /// <returns>The updated control status.</returns>
    /// <response code="200">Evidence reviewed.</response>
    /// <response code="404">Control not found.</response>
    [HttpPost("{id:guid}/evidence/{evidenceId:guid}/review")]
    [ProducesResponseType(typeof(ControlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ControlDto>> ReviewEvidence(
        Guid id, Guid evidenceId, [FromBody] ReviewEvidenceRequest request)
    {
        var result = await _mediator.Send(new ReviewEvidenceCommand(id, evidenceId, request.Outcome, request.Reviewer));
        return Ok(result);
    }

    private static ComplianceFramework? ParseFramework(string? framework) => framework?.Trim().ToLowerInvariant() switch
    {
        "soc2" => ComplianceFramework.Soc2,
        "iso27001" => ComplianceFramework.Iso27001,
        null or "" => null,
        _ => throw new ArgumentOutOfRangeException(nameof(framework), "Must be 'Soc2' or 'Iso27001'.")
    };
}
