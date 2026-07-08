using MiniGrc.Agent;
using MiniGrc.Agent.Models;
using MiniGrc.Domain;
using MiniGrc.Domain.Entities;
using MiniGrc.Domain.Enums;

namespace MiniGrc.Agent;

/// <summary>
/// Application-facing agent service. Runs <see cref="ComplianceAgent"/> and persists the produced
/// findings (and their remediation tasks) through the domain <see cref="IUnitOfWork"/>, so the
/// result is queryable via the normal CQRS read paths.
/// </summary>
public sealed class ComplianceAgentService
{
    private readonly ComplianceAgent _agent;
    private readonly IUnitOfWork _uow;

    /// <summary>Constructs the service.</summary>
    public ComplianceAgentService(ComplianceAgent agent, IUnitOfWork uow)
    {
        _agent = agent;
        _uow = uow;
    }

    /// <summary>Runs the agent and persists the findings it produces.</summary>
    /// <param name="request">The agent input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The agent result (with persisted findings).</returns>
    public async Task<AgentResult> RunAndPersistAsync(AgentRequest request, CancellationToken ct = default)
    {
        var result = await _agent.RunAsync(request, ct);

        foreach (var f in result.Findings)
        {
            // De-duplicate by external id so re-runs are idempotent.
            var existing = await _uow.Findings.GetByExternalIdAsync(f.ExternalId, ct);
            if (existing is not null) continue;

            var severity = f.Severity;
            var finding = Finding.Create(f.Title, f.Description, severity, request.Source, f.ExternalId);
            if (f.MappedControlCode is not null)
                finding.ApplyMapping(mappedControlId: null, suggestedControlCode: f.MappedControlCode);
            foreach (var r in f.Remediations)
                finding.AddRemediationTask(r.Title, r.Detail, r.Priority);
            await _uow.Findings.AddAsync(finding, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return result;
    }
}
