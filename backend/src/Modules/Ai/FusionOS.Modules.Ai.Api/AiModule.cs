using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.RecordRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Infrastructure.Persistence;
using FusionOS.Modules.Ai.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Ai.Api;

/// <summary>
/// Phase 7 — AI Platform, first slice (2026-07-18). Registers the module's DbContext,
/// Recommendations CQRS, repository, and the outbox dispatcher that relays
/// RecommendationCreated/RecommendationAccepted to Kafka (03_SYSTEM_ARCHITECTURE.md §4.2).
/// AI publishes events but consumes none this slice, so no IIntegrationEventConsumer is
/// registered here — consistent with 12_AI_PLATFORM.md §3's requirement that AI never sit
/// in the synchronous critical path of a transactional module.
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

        services.AddScoped<IRecommendationRepository, RecommendationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(RecordRecommendationCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(AiModule).Assembly);

        services.AddHostedService<OutboxDispatcher<AiDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
