using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniGrc.Agent;
using MiniGrc.Agent.Models;
using MiniGrc.Api.Requests;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Api.Controllers;

/// <summary>
/// Endpoints that run the compliance agent over a security-tool export or policy document.
/// </summary>
[ApiController]
[Route("api/v1/agent")]
[Produces("application/json")]
public sealed class AgentController : ControllerBase
{
    private readonly ComplianceAgentService _agent;

    /// <summary>Constructs the controller.</summary>
    public AgentController(ComplianceAgentService agent) => _agent = agent;

    /// <summary>
    /// Runs the agent against the supplied input. The agent maps findings to SOC 2 / ISO 27001
    /// controls, drafts remediation tasks, and writes a risk summary. It uses an LLM when one is
    /// configured, otherwise falls back to a deterministic analyzer (always succeeds).
    /// </summary>
    /// <param name="request">The agent input (source, format, content, framework).</param>
    /// <returns>The agent run result.</returns>
    /// <response code="200">Agent run completed (see <c>usedLlm</c> to know which path ran).</response>
    [HttpPost("run")]
    [ProducesResponseType(typeof(AgentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentResult>> Run([FromBody] RunAgentRequest request)
    {
        if (!Enum.TryParse<ComplianceFramework>(request.Framework, ignoreCase: true, out var framework))
            return BadRequest("Framework must be 'Soc2' or 'Iso27001'.");

        var agentRequest = new AgentRequest(request.Source, request.Format, request.Content, framework);
        var result = await _agent.RunAndPersistAsync(agentRequest);
        return Ok(result);
    }
}
