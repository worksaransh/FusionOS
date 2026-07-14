using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Quality.Api;

/// <summary>
/// Structural registration only — reserved for Phase 5 — Quality & Maintenance. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class QualityModule : IModule
{
    public string Name => "Quality";
    public string RoadmapPhase => "Phase 5 — Quality & Maintenance";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<QualityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "quality")));

        services.AddControllers().AddApplicationPart(typeof(QualityModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
