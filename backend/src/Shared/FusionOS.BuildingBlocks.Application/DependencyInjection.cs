using System.Reflection;
using FluentValidation;
using FusionOS.BuildingBlocks.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.BuildingBlocks.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR + FluentValidation + the standard pipeline for a module's
    /// Application assembly. Pipeline order is deliberate: log everything first,
    /// reject invalid input before checking permissions, audit only what succeeds.
    /// </summary>
    public static IServiceCollection AddModuleApplication(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(TenantIsolationBehavior<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(AuditBehavior<,>));

        return services;
    }
}
