using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.BusinessIntelligence.Api;

/// <summary>
/// Structural registration only — reserved for Phase 6 — Business Intelligence. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class BusinessIntelligenceModule : IModule
{
    public string Name => "BusinessIntelligence";
    public string RoadmapPhase => "Phase 6 — Business Intelligence";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BusinessIntelligenceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "bi")));

        services.AddControllers().AddApplicationPart(typeof(BusinessIntelligenceModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
