using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniGrc.Application.DTOs;
using MiniGrc.Application.Queries;

namespace MiniGrc.Api.Controllers;

/// <summary>
/// Read endpoints for the compliance dashboard and the risk register.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Constructs the controller with the MediatR mediator.</summary>
    public DashboardController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns the aggregated compliance status (coverage, counts, per-framework rollup).</summary>
    /// <returns>The compliance status summary.</returns>
    /// <response code="200">Returns the status.</response>
    [HttpGet("compliance/status")]
    [ProducesResponseType(typeof(ComplianceStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComplianceStatusDto>> GetComplianceStatus()
        => Ok(await _mediator.Send(new GetComplianceStatusQuery()));

    /// <summary>Returns the risk register.</summary>
    /// <returns>The list of risks.</returns>
    /// <response code="200">Returns the risks.</response>
    [HttpGet("risks")]
    [ProducesResponseType(typeof(IReadOnlyList<RiskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RiskDto>>> GetRisks()
        => Ok(await _mediator.Send(new GetRisksQuery()));
}
