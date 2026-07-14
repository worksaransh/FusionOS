using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.IntegrationHub.Api;

/// <summary>
/// Structural registration only — reserved for Phase 9 — Integrations & Mobile. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class IntegrationHubModule : IModule
{
    public string Name => "IntegrationHub";
    public string RoadmapPhase => "Phase 9 — Integrations & Mobile";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IntegrationHubDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "integration_hub")));

        services.AddControllers().AddApplicationPart(typeof(IntegrationHubModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
