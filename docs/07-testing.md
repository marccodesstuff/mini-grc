# Testing

Two test projects. Two targets: unit speed and real-browser coverage.

## Unit tests

Run handler rules against in-memory service fakes. No EF, no Postgres, no browser.

```bash
dotnet test tests/MiniGrc.UnitTests/MiniGrc.UnitTests.csproj
```

Current: 4 handler tests passing.

## E2E tests

Playwright drives the live API + Web apps over `data-testid` selectors only.

Flows:
- create-control
- upload-evidence
- run-agent
- view-status

```bash
# applications must be running
dotnet test tests/MiniGrc.E2E/MiniGrc.E2E.csproj
```

Current: 5 Playwright tests passing.

## Selector rule

E2E tests never use CSS class or structure selectors. Only `data-testid`.
