using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MiniGrc.Infrastructure.Persistence;

namespace MiniGrc.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations</c> to construct <see cref="MiniGrcDbContext"/>
/// without booting the full application host. Uses a local Postgres connection by default; the
/// connection string can be overridden with the <c>ConnectionStrings__MiniGrc</c> environment variable.
/// </summary>
public sealed class MiniGrcDbContextFactory : IDesignTimeDbContextFactory<MiniGrcDbContext>
{
    /// <inheritdoc/>
    public MiniGrcDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MiniGrc")
            ?? "Host=localhost;Port=5432;Database=minigrc;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<MiniGrcDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new MiniGrcDbContext(optionsBuilder.Options);
    }
}
