using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Manufacturing.Api;

/// <summary>
/// Structural registration only — reserved for Phase 3 — Manufacturing ERP. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class ManufacturingModule : IModule
{
    public string Name => "Manufacturing";
    public string RoadmapPhase => "Phase 3 — Manufacturing ERP";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ManufacturingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "manufacturing")));

        services.AddControllers().AddApplicationPart(typeof(ManufacturingModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
