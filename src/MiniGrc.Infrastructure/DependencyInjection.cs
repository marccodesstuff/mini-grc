using Microsoft.EntityFrameworkCore;
using MiniGrc.Domain;
using MiniGrc.Infrastructure.Persistence;
using MiniGrc.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MiniGrc.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. Wires the EF Core DbContext to PostgreSQL and
/// registers the concrete <c>IUnitOfWork</c> implementation behind the Domain port.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds the Infrastructure layer services to the container.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MiniGrcDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(MiniGrcDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
