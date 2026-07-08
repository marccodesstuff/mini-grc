using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniGrc.Application.DTOs;
using MiniGrc.Application.Queries;

namespace MiniGrc.Api.Controllers;

/// <summary>
/// Read endpoints for security findings produced and mapped by the compliance agent.
/// </summary>
[ApiController]
[Route("api/v1/findings")]
[Produces("application/json")]
public sealed class FindingsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Constructs the controller with the MediatR mediator.</summary>
    public FindingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists findings, optionally filtered to those the agent has not yet mapped.</summary>
    /// <param name="onlyUnmapped">When true, returns only unmapped findings.</param>
    /// <returns>The collection of findings.</returns>
    /// <response code="200">Returns the findings.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FindingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FindingDto>>> GetAll([FromQuery] bool onlyUnmapped = false)
        => Ok(await _mediator.Send(new GetFindingsQuery(onlyUnmapped)));
}
