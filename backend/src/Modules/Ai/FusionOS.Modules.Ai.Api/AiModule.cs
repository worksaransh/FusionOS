using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.Modules.Ai.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Ai.Api;

/// <summary>
/// Structural registration only — reserved for Phase 7 — AI Platform. Wires the module's (empty)
/// DbContext and health endpoint into the Host so the modular-monolith shape
/// (03_SYSTEM_ARCHITECTURE.md) is provable end-to-end today, before any business
/// logic exists.
/// </summary>
public sealed class AiModule : IModule
{
    public string Name => "Ai";
    public string RoadmapPhase => "Phase 7 — AI Platform";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AiDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "ai")));

        services.AddControllers().AddApplicationPart(typeof(AiModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
