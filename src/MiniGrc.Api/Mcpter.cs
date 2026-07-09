using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MiniGrc.Application;
using MiniGrc.Application.Queries;

namespace MiniGrc.Api;

public static class McpModule
{
    public static IServiceCollection AddMiniGrcMcp(this IServiceCollection services)
    {
        services.AddApplication();
        return services;
    }

    public static IApplicationBuilder UseMiniGrcMcp(this IApplicationBuilder app)
    {
        app.MapGet("/mcp/status", async context =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { status = "ok", message = "MCP endpoint placeholder" });
        });
        return app;
    }
}
