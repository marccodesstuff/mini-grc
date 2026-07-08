using MiniGrc.Web;
using MiniGrc.Web.Components;
using MiniGrc.Web.Services;

namespace MiniGrc.Web;

/// <summary>Blazor Web (server interactivity) entry point and composition root.</summary>
public sealed class Program
{
    /// <summary>Application entry point.</summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Server-side rendering with interactive server components.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Typed client that talks to the Mini-GRC API (runs on :5050 in dev).
        var apiBase = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5050";
        builder.Services.AddScoped<ApiClient>(_ => new ApiClient(new HttpClient { BaseAddress = new Uri(apiBase) }));

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/Error", createScopeForErrors: true);

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
