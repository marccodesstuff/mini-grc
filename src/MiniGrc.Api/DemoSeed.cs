using Microsoft.Extensions.DependencyInjection;
using MiniGrc.Application.Commands;
using MiniGrc.Domain;
using MiniGrc.Domain.Enums;
using MediatR;

namespace MiniGrc.Api;

/// <summary>
/// Seeds a small, realistic demo dataset on first run in development so the dashboard and agent
/// are immediately explorable. Idempotent: it checks for existing controls before inserting.
/// </summary>
public static class DemoSeed
{
    /// <summary>Runs the seed using the scoped service provider.</summary>
    public static async Task RunAsync(IServiceProvider provider)
    {
        var uow = provider.GetRequiredService<IUnitOfWork>();
        var mediator = provider.GetRequiredService<IMediator>();

        if (await uow.Controls.GetAllAsync() is { Count: > 0 })
            return;

        await mediator.Send(new CreateControlCommand("SOC2-CC6.1", "Logical Access Control", "Enforce MFA and least-privilege access.", ComplianceFramework.Soc2, "SecOps"));
        await mediator.Send(new CreateControlCommand("SOC2-CC7.1", "Vulnerability Management", "Scan dependencies and patch critical CVEs.", ComplianceFramework.Soc2, "Platform"));
        await mediator.Send(new CreateControlCommand("ISO-A.8.8", "Technical Vulnerability Management", "Track and remediate vulnerabilities from scans.", ComplianceFramework.Iso27001, "Platform"));
        await mediator.Send(new CreateControlCommand("ISO-A.8.15", "Logging", "Centralize audit logs for access and changes.", ComplianceFramework.Iso27001, "SecOps"));
    }
}
