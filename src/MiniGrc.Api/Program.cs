using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MiniGrc.Api.OpenApi;
using MiniGrc.Api.Requests;
using System.Text.Json.Serialization;
using MiniGrc.Application;
using MiniGrc.Domain;
using MiniGrc.Infrastructure;
using MiniGrc.Infrastructure.Persistence;
using MiniGrc.Agent;
using MediatR;
using MiniGrc.Application.Commands;

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
            ?? "Host=localhost;Port=5432;Database=minigrc;Username=postgres;Password=postgres";

        // ---- Onion layers ----
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddAgent(builder.Configuration);

        // ---- MVC / controllers ----
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Serialize enums as strings (e.g. "Soc2", "Verified") so the API and the
                // OpenAPI document are human-readable and match what the Blazor front end sends.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

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
        app.MapControllers();
        app.MapOpenApi();

        app.Run();
    }

    /// <summary>Builds the web application without running it (used by integration tests / E2E).</summary>
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("MiniGrc")
            ?? "Host=localhost;Port=5432;Database=minigrc;Username=postgres;Password=postgres";
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddAgent(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        var app = builder.Build();
        app.UseRouting();
        app.MapControllers();
        app.MapOpenApi();
        return app;
    }
}
