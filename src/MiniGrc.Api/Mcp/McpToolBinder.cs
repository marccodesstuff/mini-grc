using System.Text.Json;
using MiniGrc.Application;
using MiniGrc.Application.Commands;
using MiniGrc.Application.Queries;
using MiniGrc.Agent.Models;
using MiniGrc.Agent;
using MiniGrc.Domain.Enums;
using MediatR;

namespace MiniGrc.Api.Mcp;

public sealed record McpToolRequest(string Name, JsonElement Arguments);

public sealed record McpToolResult(string Id, object? Result, string? Error = null);

public sealed class McpToolBinder
{
    private readonly IMediator _mediator;
    private readonly ComplianceAgentService _agent;

    public McpToolBinder(IMediator mediator, ComplianceAgentService agent)
    {
        _mediator = mediator;
        _agent = agent;
    }

    public IReadOnlyList<object> ListTools() => new object[]
    {
        new { name = "controls.list", description = "List all controls, optionally filtered by framework.", inputSchema = new { type = "object", properties = new { framework = new { type = "string", description = "Soc2 or Iso27001" } } } },
        new { name = "control.create", description = "Create a compliance control.", inputSchema = new { type = "object", properties = new { code = new { type = "string" }, title = new { type = "string" }, framework = new { type = "string" } } } },
        new { name = "agent.run", description = "Run the compliance agent over security findings / policy prose.", inputSchema = new { type = "object", properties = new { source = new { type = "string" }, format = new { type = "string" }, content = new { type = "string" }, framework = new { type = "string" } } } },
    };

    public async Task<McpToolResult> CallAsync(McpToolRequest req, CancellationToken ct)
    {
        return req.Name switch
        {
            "controls.list" => await ControlsList(req.Arguments, ct),
            "control.create" => await ControlCreate(req.Arguments, ct),
            "agent.run" => await AgentRun(req.Arguments, ct),
            _ => new McpToolResult(Guid.NewGuid().ToString(), null, $"Unknown tool: {req.Name}")
        };
    }

    private async Task<McpToolResult> ControlsList(JsonElement args, CancellationToken ct)
    {
        string? framework = null;
        if (args.TryGetProperty("framework", out var f) && f.ValueKind == JsonValueKind.String)
            framework = f.GetString();
        var result = await _mediator.Send(new GetControlsQuery(ParseFramework(framework)), ct);
        return new McpToolResult(Guid.NewGuid().ToString(), result);
    }

    private async Task<McpToolResult> ControlCreate(JsonElement args, CancellationToken ct)
    {
        var code = args.GetProperty("code").GetString() ?? "";
        var title = args.GetProperty("title").GetString() ?? "";
        var desc = args.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : "";
        var framework = Enum.Parse<ComplianceFramework>(args.GetProperty("framework").GetString() ?? "Soc2", true);
        var owner = args.TryGetProperty("owner", out var o) && o.ValueKind == JsonValueKind.String ? o.GetString() : "";
        var result = await _mediator.Send(new CreateControlCommand(code, title, desc, framework, owner), ct);
        return new McpToolResult(Guid.NewGuid().ToString(), result);
    }

    private async Task<McpToolResult> AgentRun(JsonElement args, CancellationToken ct)
    {
        var source = args.GetProperty("source").GetString() ?? "";
        var format = args.GetProperty("format").GetString() ?? "json";
        var content = args.GetProperty("content").GetString() ?? "";
        var framework = Enum.Parse<ComplianceFramework>(args.GetProperty("framework").GetString() ?? "Soc2", true);
        var req2 = new AgentRequest(source, format, content, framework);
        var result = await _agent.RunAndPersistAsync(req2, ct);
        return new McpToolResult(Guid.NewGuid().ToString(), result);
    }

    private static ComplianceFramework? ParseFramework(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "soc2" => ComplianceFramework.Soc2,
        "iso27001" or "iso" => ComplianceFramework.Iso27001,
        _ => null
    };
}
