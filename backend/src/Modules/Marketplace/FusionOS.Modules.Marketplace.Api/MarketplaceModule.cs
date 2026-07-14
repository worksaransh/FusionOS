using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.Marketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Marketplace.Api;

/// <summary>
/// Structural registration only — reserved for Phase 8 — Marketplace & Ecosystem. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class MarketplaceModule : IModule
{
    public string Name => "Marketplace";
    public string RoadmapPhase => "Phase 8 — Marketplace & Ecosystem";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MarketplaceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "marketplace")));

        services.AddControllers().AddApplicationPart(typeof(MarketplaceModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
