# MCP bridge

A thin JSON-RPC bridge at **`POST /mcp`** exposes Mini-GRC tools to any MCP-compatible client without locking to a specific MCP server SDK.

## Tools

- `controls.list` → `controls.list(framework?)`
- `control.create` → `control.create(code, title, framework, description?, owner?)`
- `agent.run` → `agent.run(source, format, content, framework)`

## Request/response

Request:

```json
{ "jsonrpc": "2.0", "id": 1, "method": "tools/list" }
```

Response:

```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "result": {
    "tools": [
      { "name": "controls.list", "description": "...", "inputSchema": { ... } },
      { "name": "control.create", "description": "...", "inputSchema": { ... } },
      { "name": "agent.run", "description": "...", "inputSchema": { ... } }
    ]
  }
}
```

Tool call:

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "agent.run",
    "arguments": {
      "source": "dependabot",
      "format": "json",
      "content": "{\"findings\":[...]}",
      "framework": "Soc2"
    }
  }
}
```

## Design choice

Instead of self-hosting a sealed MCP server, Mini-GRC implements the protocol surface it actually needs: `tools/list` and `tools/call`.

The same MediatR/agent pipeline serves the REST API, Blazor UI, and MCP clients. `McpToolBinder` routes tool names to existing commands/queries. No duplicate orchestration, no opaque SDK runtime, no stdio/SSE transport constraints.

## Runtime behavior

- Route: `MapPost("/mcp", ...)`
- DI: `AddScoped<McpToolBinder>()`
- Unknown methods return JSON-RPC error `-32601`.
- Handler exceptions return error `-32603`.
- `tools/call` always wraps the result in `{ content: [{ type: "text", text: ... }] }`.
