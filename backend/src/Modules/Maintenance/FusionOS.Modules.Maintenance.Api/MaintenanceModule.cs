using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Maintenance.Api;

/// <summary>
/// Structural registration only — reserved for Phase 5 — Quality & Maintenance. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class MaintenanceModule : IModule
{
    public string Name => "Maintenance";
    public string RoadmapPhase => "Phase 5 — Quality & Maintenance";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MaintenanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "maintenance")));

        services.AddControllers().AddApplicationPart(typeof(MaintenanceModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
