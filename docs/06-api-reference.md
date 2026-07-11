# API reference

Base URL: `/api/v1`  
OpenAPI 3.1.1 spec: `/openapi/v1.json`  
Swagger UI: `/swagger`  
MCP bridge: `/mcp`

## Controls

- `GET /api/v1/controls` — list all controls, optional `?framework=Soc2|Iso27001`
- `GET /api/v1/controls/{id}` — single control
- `POST /api/v1/controls` — create control
- `PUT /api/v1/controls/{id}` — update title / description / owner
- `POST /api/v1/controls/{id}/evidence` — attach evidence metadata
- `POST /api/v1/controls/{id}/evidence/{evidenceId}/review` — approve or reject evidence

## Agent

- `POST /api/v1/agent/run` — run compliance agent

Body:

```json
{
  "source": "dependabot",
  "format": "json",
  "content": "{ ... }",
  "framework": "Soc2"
}
```

## Dashboard / findings

- `GET /api/v1/compliance/status` — framework compliance rollup
- `GET /api/v1/risks` — risk register
- `GET /api/v1/findings` — agent findings, optional `?onlyUnmapped=true`

## Conventions

- `Content-Type: application/json`
- Enums serialized as strings via `JsonStringEnumConverter`
- `/mcp` returns JSON-RPC 2.0 objects for `tools/list` and `tools/call`
