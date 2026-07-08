using FluentValidation;
using MediatR;
using MiniGrc.Application.Behaviors;
using MiniGrc.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace MiniGrc.Application;

/// <summary>
/// Composition root for the Application layer. Registers MediatR (with pipeline behaviors),
/// FluentValidation, and the Mapster mapping configuration. The Infrastructure layer registers
/// the concrete <c>IUnitOfWork</c> implementation separately.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds the Application layer services to the container.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR scans this assembly for handlers.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Pipeline order: validation runs first, then logging wraps the handler.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // FluentValidation validators in this assembly.
        services.AddValidatorsFromAssembly(assembly);

        // Register Mapster entity->DTO adapters.
        MappingConfig.Register();

        return services;
    }
}
