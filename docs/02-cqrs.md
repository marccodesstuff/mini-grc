# CQRS

Mini-GRC uses MediatR for command/query separation with two pipeline behaviors: validation via FluentValidation, and request logging.

## Commands (writes)

```csharp
// Create a control
await mediator.Send(new CreateControlCommand(
    Code: "SOC2-CC6.1",
    Title: "Logical Access Control",
    Description: "Enforce MFA...",
    Framework: ComplianceFramework.Soc2,
    Owner: "SecOps"));

// Attach evidence metadata
await mediator.Send(new AttachEvidenceCommand(
    controlId, fileName, contentType, sizeBytes, uploadedBy));

// Review evidence
await mediator.Send(new ReviewEvidenceCommand(
    controlId, evidenceId, ReviewOutcome.Approved, reviewer));
```

## Queries (reads)

```csharp
// List controls, optionally filtered
var controls = await mediator.Send(new GetControlsQuery(ComplianceFramework.Soc2));

// Compliance status rollup
var status = await mediator.Send(new GetComplianceStatusQuery());

// Risk register
var risks = await mediator.Send(new GetRisksQuery());
```

## Pipeline

1. `ValidationBehavior` runs FluentValidation validators before the handler.
2. `LoggingBehavior` logs request type and latency to console (zero external logging dependency).
3. Handler executes. Maps to/from DTOs via Mapster.

## Mapping

`MappingConfig.Register()` sets up Mapster once at startup. No per-handler mapping code.

## Why CQRS fits here

- Reads (`GetControlsQuery`) can be cached or extended with projections without affecting writes.
- Validation is centralized in the pipeline, not scattered across controllers.
- The Agent and MCP bridge reuse the exact same commands/queries — no duplicate orchestration.
