using FluentAssertions;
using MediatR;
using MiniGrc.Application.Commands;
using MiniGrc.Application.DTOs;
using MiniGrc.Application.Handlers;
using MiniGrc.Application.Queries;
using MiniGrc.Domain;
using MiniGrc.Domain.Entities;
using MiniGrc.Domain.Enums;
using MiniGrc.UnitTests.Fakes;
using Xunit;

namespace MiniGrc.UnitTests.Handlers;

/// <summary>Exercises the control command/query handlers against an in-memory unit of work.</summary>
public sealed class ControlHandlersTests
{
    private readonly IUnitOfWork _uow = new InMemoryUnitOfWork();

    static ControlHandlersTests()
    {
        // Mirror the production composition root: handlers rely on Mapster entity->DTO adapters.
        MiniGrc.Application.Mappings.MappingConfig.Register();
    }

    [Fact]
    public async Task CreateControl_Persists_And_Returns_Dto()
    {
        var handler = new CreateControlHandler(_uow);
        var command = new CreateControlCommand("SOC2-CC6.1", "Access Control", "Logical access reviews", ComplianceFramework.Soc2, "SecOps");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBe(Guid.Empty);
        result.Code.Should().Be("SOC2-CC6.1");
        result.Status.Should().Be(nameof(ControlStatus.NotImplemented));
        (await _uow.Controls.GetAllAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task AttachEvidence_Approved_Upgrades_Status_To_Verified()
    {
        var control = Control.Create("SOC2-CC6.1", "Access Control", "desc", ComplianceFramework.Soc2, "SecOps");
        await _uow.Controls.AddAsync(control);
        await _uow.SaveChangesAsync();

        var evidence = await new AttachEvidenceHandler(_uow)
            .Handle(new AttachEvidenceCommand(control.Id, "policy.pdf", "application/pdf", 1024, "alice"), CancellationToken.None);

        var reviewed = await new ReviewEvidenceHandler(_uow)
            .Handle(new ReviewEvidenceCommand(control.Id, evidence.Id, EvidenceStatus.Approved, "bob"), CancellationToken.None);

        reviewed.Status.Should().Be(nameof(ControlStatus.Verified));
        reviewed.ApprovedEvidenceCount.Should().Be(1);
    }

    [Fact]
    public async Task GetComplianceStatus_Computes_Coverage()
    {
        var c1 = Control.Create("A", "a", "", ComplianceFramework.Soc2, "o");
        c1.AttachEvidence("e.pdf", "application/pdf", 10, "u");
        c1.ReviewEvidence(c1.Evidence[0].Id, EvidenceStatus.Approved, "rev");
        var c2 = Control.Create("B", "b", "", ComplianceFramework.Iso27001, "o");
        await _uow.Controls.AddAsync(c1);
        await _uow.Controls.AddAsync(c2);
        await _uow.SaveChangesAsync();

        var status = await new GetComplianceStatusHandler(_uow)
            .Handle(new GetComplianceStatusQuery(), CancellationToken.None);

        status.TotalControls.Should().Be(2);
        status.VerifiedControls.Should().Be(1);
        status.CoveragePercent.Should().Be(50.0);
        status.ByFramework.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateControl_Throws_When_Missing()
    {
        var act = async () => await new UpdateControlHandler(_uow)
            .Handle(new UpdateControlCommand(Guid.NewGuid(), "x", "", "o"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
