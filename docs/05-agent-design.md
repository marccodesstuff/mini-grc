# Agent design

`MiniGrc.Agent` turns raw security input into mapped, actionable findings. This is the “AI orchestration / autonomous agent” portion.

## Flow

1. Caller supplies `source`, `format` (`json` / `text`), `content`, and `framework`.
2. If `Agent:LlmEndpoint` is set, `ComplianceAgent` sends a framed prompt and expects strict JSON.
3. If the LLM is unreachable, times out, or returns malformed JSON, the agent degrades to `DeterministicAnalyzer`.
4. Findings are mapped to controls via `ControlCatalog.MapToControlCode`.
5. Remediation tasks are drafted with priority scaled to severity.
6. `ComplianceAgentService` persists results through `IUnitOfWork`.

## Components

- `ComplianceAgentService` — entrypoint; persists findings.
- `ComplianceAgent` — orchestration; accepts a strategy (LLM or deterministic).
- `OpenAiCompatibleClient` — minimal OpenAI-compatible client (`/v1/chat/completions`).
- `DeterministicAnalyzer` — offline-safe fallback brain.
- `ControlCatalog` — static control library; keyword-based mapping.
- `AgentModels` — `AgentRequest`, `AgentResult`, `AgentFinding`, `AgentRemediation`.

## Failure modes

- No `Agent:LlmEndpoint` → deterministic-only path.
- LLM unreachable / timeout → catch, fallback, log.
- Malformed LLM response → strip fences, reparse; still fallback if parse fails.
- Product always produces a result; the LLM is an enhancement.

## Design choices

- Prompt framing is fixed, not dynamic — failures are versioned and reviewable.
- No streaming; bounded synchronous runs keep orchestration inspectable.
