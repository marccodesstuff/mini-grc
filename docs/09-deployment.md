# Deployment

## Env vars

- `ConnectionStrings__MiniGrc`
- `Agent__LlmEndpoint`
- `Agent__Model`

## Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
COPY bin/ .
ENTRYPOINT ["dotnet", "MiniGrc.Api.dll"]
```

```bash
dotnet publish src/MiniGrc.Api/MiniGrc.Api.csproj -c Release -o ./publish
docker build -t minigrc-api -f src/MiniGrc.Api/Dockerfile ./publish
```

```bash
docker run -p 5050:8080 \
  -e ConnectionStrings__MiniGrc="Host=pg;Port=5432;Database=minigrc;Username=postgres;Password=***" \
  minigrc-api
```

## Notes

- `MiniGrc.Web` is a separate deployment.
- In production, gate `db.Database.Migrate()` behind a release step, not startup.
