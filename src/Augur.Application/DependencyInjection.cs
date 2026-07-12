using FluentValidation;
using MediatR;
using Augur.Application.Behaviors;
using Augur.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Augur.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddValidatorsFromAssembly(assembly);
        MappingConfig.Register();

        return services;
    }
}
