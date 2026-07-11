# Setup

## Prerequisites

- .NET 10 SDK
- PostgreSQL 16+
- Node.js + npm (scss compile only)
- LM Studio optional

## Database

Docker:

```bash
docker run -d --name minigrc-pg \
  -e POSTGRES_PASSWORD=1234 \
  -e POSTGRES_DB=minigrc \
  -p 5432:5432 postgres:16
```

Match `appsettings.json` and `Program.cs` fallback password.

## Run

```bash
# Terminal 1: API
dotnet run --project src/MiniGrc.Api/MiniGrc.Api.csproj --urls http://localhost:5050

# Terminal 2: Web
dotnet run --project src/MiniGrc.Web/MiniGrc.Web.csproj --urls http://localhost:5000
```

## Compile styles

```bash
cd src/MiniGrc.ComponentLib
npm install
npx sass
```

## Troubleshooting

- `28P01`: password mismatch. Update both `appsettings.json` and `Program.cs` fallback.
- `MSB3021`/`MSB3027`: running `dotnet run` holds DLL locks. Kill `dotnet.exe` and rebuild.
- `/mcp` 405: use `POST`.
- `/mcp` 404: route mounted after `MapControllers()` in `Program.cs`.
