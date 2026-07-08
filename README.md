# Mini-GRC — miniature compliance automation platform

A small SaaS that tracks security controls and compliance evidence, with an AI agent that does the
smart part: ingesting policy documents / security-tool exports and mapping findings onto SOC 2 and
ISO 27001 controls. Built to mirror **Compyl's** problem space and exact stack
(**C# / .NET 10, Onion Architecture, CQRS via MediatR, EF Core + PostgreSQL, Blazor, OpenAPI 3.1.1,
Playwright, an LLM agent layer**).

> This is a portfolio / screening project. It is intentionally small but every feature maps to a
> real Compyl requirement so it can be discussed end-to-end in an interview.

---

## Feature → requirement map

| Milestone | What it demonstrates | Compyl signal |
|-----------|----------------------|---------------|
| M1 Onion | Domain / Application / Infrastructure / API split with dependency direction enforced | "Onion Architecture on the backend" |
| M2 CQRS | MediatR commands + queries, validation pipeline, EF Core repositories, migrations | "writing efficient CQRS handlers", "relational databases" |
| M3 Blazor | Control library, evidence upload, compliance dashboard; shared **CompylBase** component lib; **component-scoped `.scss` only**; `data-testid` on interactives | "developing Blazor components", "no inline styling", "CompylBase components" |
| M4 OpenAPI | Native **OpenAPI 3.1.1** document with **mandatory XML docs on every endpoint** injected into the spec | "Strict adherence to OpenAPI 3.1.1 with mandatory XML documentation" |
| M5 Agent | LLM agent that ingests JSON/policy input, maps findings → controls, drafts remediation + risk summary; **offline-safe deterministic fallback** | "building secure, autonomous software agents", "AI orchestration" |
| M6 Playwright | E2E over the `data-testid` selectors (create control → upload evidence → run agent → view status) | "automated testing (Playwright)", "selectors perfectly mapped" |
| M7 Polish | Zero nullable warnings, clean build, this README | "clean up nullable warnings daily", "The Campground Rule" |

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│  MiniGrc.Web (Blazor, server interactivity)  ·  MiniGrc.ComponentLib   │  Presentation
└───────────────┬──────────────────────────────────────────────────────┘
                │ HttpClient (typed ApiClient)
┌───────────────▼──────────────────────────────────────────────────────┐
│  MiniGrc.Api (ASP.NET Core, controllers, OpenAPI 3.1.1, Swagger UI)    │  API / Host
└───────────────┬──────────────────────────────────────────────────────┘
                │ MediatR
┌───────────────▼──────────────────────────────────────────────────────┐
│  MiniGrc.Application  (CQRS handlers, validators, pipeline, mappings)  │  Application
└───────┬───────────────────────────────┬──────────────────────────────┘
        │ ports (IUnitOfWork, I*Repository)│
┌───────▼───────────────┐  ┌──────────────▼───────────────────────────┐
│ MiniGrc.Domain        │  │ MiniGrc.Infrastructure (EF Core + Npgsql)  │
│ entities, enums,      │  │ DbContext, repository impls, migrations     │
│ repository *ports*    │  └──────────────────────┬─────────────────────┘
└───────────────────────┘                         │ PostgreSQL
                                         ┌─────────▼─────────┐
                                         │  MiniGrc.Agent     │  (references Application + Domain)
                                         │ LLM client +       │
                                         │ deterministic brain│
                                         └────────────────────┘
```

**Why Onion (the interview answer):** dependencies only point *inward*. The Domain owns the
entities and the abstract repository/unit-of-work **ports** — it has no idea EF Core or PostgreSQL
exist. The Infrastructure layer is the *only* place that knows about a database provider; it
implements those ports. The Application layer holds use-case logic (CQRS handlers) and depends on
the ports, never on Infrastructure. The API/Web are the outer shell that wires everything together
in `Program.cs`. This keeps business rules testable without a database and lets the persistence
technology be swapped without touching domain logic.

---

## The CQRS flow (end to end)

1. A Blazor page calls `ApiClient` → `POST /api/v1/controls`.
2. The controller turns the JSON body into a `CreateControlCommand` and calls `IMediator.Send(...)`.
3. MediatR runs the pipeline: **`ValidationBehavior`** (FluentValidation) → **`LoggingBehavior`** → handler.
4. `CreateControlHandler` uses the `IUnitOfWork` port to add a `Control` aggregate and `SaveChangesAsync()`.
5. The handler returns a `ControlDto` (via Mapster). The controller returns `201 Created`.

No handler ever touches `DbContext` or `Npgsql` directly — only the `IUnitOfWork` abstraction.

---

## The agent (M5) — design & failure modes

`MiniGrc.Agent` turns raw security input into mapped, actionable findings.

- **Input:** a source name, a format (`json` for tool exports, `text` for policy prose), the raw
  payload, and a target framework (`Soc2` / `Iso27001`).
- **LLM path:** if `Agent:LlmEndpoint` is configured, `OpenAiCompatibleClient` (OpenAI-compatible —
  works with LM Studio, Ollama, OpenAI) sends a framed prompt and asks for strict JSON
  (`findings[]` + `risk_summary`). The response is parsed defensively (markdown fences stripped).
- **Deterministic fallback:** if the LLM is unreachable or returns unusable JSON, the agent degrades
  to `DeterministicAnalyzer`, which parses the input with a `ControlCatalog` keyword matcher and
  writes a plain risk summary. **The product always produces a result** — the LLM is an enhancement,
  never a hard dependency. This is the key design decision and the answer to "what are its failure
  modes": network down, model timeout, malformed JSON → graceful fallback, logged, still useful.
- **Mapping:** findings are matched to the best control code by keyword score
  (`ControlCatalog.MapToControlCode`). Remediation tasks are drafted with priority scaled to severity.
- **Persistence:** `ComplianceAgentService` persists findings (de-duplicated by `ExternalId`) through
  `IUnitOfWork`, so the agent output is queryable via the normal CQRS read paths.

**Run it offline:** leave `Agent:LlmEndpoint` empty in `appsettings.json` → deterministic only.
**Run it with a local model:** point `Agent:LlmEndpoint` at LM Studio's
`http://localhost:1234/v1` and set `Agent:Model` to your loaded model id.

---

## Prerequisites

- .NET 10 SDK
- PostgreSQL (or Docker)
- Node.js (only for the `.scss` → `.razor.css` compile step and Playwright browser install)
- LM Studio **optional** (agent works without it)

## Setup

```bash
# 1. Postgres (Docker)
docker run -d --name minigrc-pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=minigrc -p 5432:5432 postgres:16

# 2. Apply migrations + run the API (seeds 4 demo controls on first run)
dotnet run --project src/MiniGrc.Api/MiniGrc.Api.csproj --urls http://localhost:5050

# 3. In another terminal, run the Blazor front end (expects the API on :5050)
dotnet run --project src/MiniGrc.Web/MiniGrc.Web.csproj --urls http://localhost:5000
```

- API docs: http://localhost:5050/swagger  ·  OpenAPI 3.1.1 spec: http://localhost:5050/openapi/v1.json
- App: http://localhost:5000  (Dashboard · Controls · Evidence · Agent)

Connection string and agent settings live in `src/MiniGrc.Api/appsettings.json`:

```json
{
  "ConnectionStrings": { "MiniGrc": "Host=localhost;Port=5432;Database=minigrc;Username=postgres;Password=postgres" },
  "Agent": { "LlmEndpoint": "", "Model": "local-model" }
}
```

## Tests

```bash
dotnet test MiniGrc.slnx                      # unit tests (handlers, in-memory UoW)
# E2E requires both apps running (see Setup):
dotnet test tests/MiniGrc.E2E/MiniGrc.E2E.csproj
```

The E2E suite drives the four core flows using **`data-testid` selectors only** (no brittle CSS),
exercising create-control → upload-evidence → run-agent → view-status against the live app.

---

## Style rules (Milestone 7)

- **Nullable reference types on everywhere**; the build is warning-clean (`dotnet build` → 0 warnings).
- **No inline styling.** Component styles live in co-located `*.razor.scss` files, compiled to
  Blazor CSS-isolation `*.razor.css` by a pre-build `sass` target in `MiniGrc.ComponentLib.csproj`.
  Interactive elements carry `data-testid` for Playwright.
- `NU1903` are advisory transitive-dependency warnings (Npgsql → `System.Security.Cryptography.Xml`,
  SwaggerUI → `Microsoft.OpenApi 2.0.0`); suppressed solution-wide in `Directory.Build.props` with a
  comment explaining why — no code change mitigates them without forking the ecosystem packages.

---

## Project layout

```
src/
  MiniGrc.Domain/        entities, enums, repository ports, IUnitOfWork
  MiniGrc.Application/   CQRS commands/queries, handlers, FluentValidation, MediatR pipeline, Mapster
  MiniGrc.Infrastructure/ EF Core DbContext, repository impls, PostgreSQL migrations
  MiniGrc.Api/           ASP.NET Core controllers, OpenAPI 3.1.1 + XML docs, agent endpoint
  MiniGrc.Agent/         LLM client, ControlCatalog, deterministic analyzer, orchestration
  MiniGrc.ComponentLib/  CompylBase shared Blazor components (.razor + .razor.scss)
  MiniGrc.Web/           Blazor front end (Dashboard, Controls, Evidence, Agent pages)
tests/
  MiniGrc.UnitTests/     handler tests against an in-memory IUnitOfWork
  MiniGrc.E2E/           Playwright flows over data-testid selectors
```
