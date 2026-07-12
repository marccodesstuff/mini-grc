using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MiniGrc.Api.OpenApi;
using MiniGrc.Api.Requests;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiniGrc.Application;
using MiniGrc.Domain;
using MiniGrc.Infrastructure;
using MiniGrc.Infrastructure.Persistence;
using MiniGrc.Agent;
using MediatR;
using MiniGrc.Application.Commands;
using MiniGrc.Api.Mcp;
using MiniGrc.Api.Auth;
using Microsoft.AspNetCore.Authentication;

namespace MiniGrc.Api;

/// <summary>
/// API host composition root. Wires the Onion layers (Application + Infrastructure), MediatR,
/// OpenAPI 3.1.1 (native generator) with XML documentation, the compliance agent, and CORS for
/// the Blazor front end.
/// </summary>
public sealed class Program
{
    /// <summary>Application entry point.</summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("MiniGrc")
            ?? "Host=localhost;Port=5432;Database=minigrc;Username=postgres;Password=1234";

        // ---- Onion layers ----
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddAgent(builder.Configuration);
        builder.Services.AddScoped<McpToolBinder>();

        // ---- MVC / controllers ----
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Serialize enums as strings (e.g. "Soc2", "Verified") so the API and the
                // OpenAPI document are human-readable and match what the Blazor front end sends.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // ---- Authentication (API key) ----
        // Every controller and the /mcp endpoint require a valid X-Api-Key header. This is the
        // trust boundary for the write operations that mutate the compliance record.
        builder.Services
            .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { });
        builder.Services.AddAuthorization();

        // ---- CORS for the Blazor front end (dev + preview) ----
        builder.Services.AddCors(options =>
            options.AddPolicy("BlazorClient", policy =>
                policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

        // ---- OpenAPI 3.1.1 (M4) with XML documentation ----
        var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        var transformer = new XmlCommentsTransformer(xmlPath);

        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.AddOperationTransformer(transformer);
            options.AddSchemaTransformer(transformer);
        });

        var app = builder.Build();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Mini-GRC API v1 (OpenAPI 3.1.1)");
            options.RoutePrefix = "swagger";
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MiniGrcDbContext>();
            db.Database.Migrate();
            DemoSeed.RunAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        }

        app.UseRouting();
        app.UseCors("BlazorClient");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
        app.MapOpenApi();
        app.MapPost("/mcp", async context =>
        {
            var binder = context.RequestServices.GetRequiredService<McpToolBinder>();
            using var doc = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
            var root = doc.RootElement;
            var method = root.GetProperty("method").GetString() ?? "";
            var id = root.TryGetProperty("id", out var idEl) ? idEl.GetRawText() : "null";
            var result = method switch
            {
                "tools/list" => new { jsonrpc = "2.0", id, result = new { tools = binder.ListTools() } },
                "tools/call" => await HandleToolCallAsync(root, binder, context.RequestAborted),
                _ => new { jsonrpc = "2.0", id, error = new { code = -32601, message = $"Method not found: {method}" } }
            };
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(result, context.RequestAborted);
        }).RequireAuthorization();
        app.Run();
    }

    private static async Task<object> HandleToolCallAsync(JsonElement root, McpToolBinder binder, CancellationToken ct)
    {
        var id = root.TryGetProperty("id", out var idEl) ? idEl.GetRawText() : "null";
        try
        {
            var paramsEl = root.GetProperty("params");
            var name = paramsEl.GetProperty("name").GetString() ?? "";
            var args = paramsEl.TryGetProperty("arguments", out var a) ? a : default;
            var callResult = await binder.CallAsync(new McpToolRequest(name, args), ct);
            if (callResult.Error is not null)
                return new { jsonrpc = "2.0", id, result = new { content = new[] { new { type = "text", text = callResult.Error } }, isError = true } };
            return new { jsonrpc = "2.0", id, result = new { content = new[] { new { type = "text", text = System.Text.Json.JsonSerializer.Serialize(callResult.Result) } } } };
        }
        catch (Exception ex)
        {
            return new { jsonrpc = "2.0", id, error = new { code = -32603, message = ex.Message } };
        }
    }

    /// <summary>Builds the web application without running it (used by integration tests / E2E).</summary>
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("MiniGrc")
            ?? "Host=localhost;Port=5432;Database=minigrc;Username=postgres;Password=1234";
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddAgent(builder.Configuration);
        builder.Services.AddScoped<McpToolBinder>();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services
            .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { });
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
        app.MapOpenApi();
        return app;
    }
}
