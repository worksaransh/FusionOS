using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.CreateKpiDefinition;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;
using FusionOS.Modules.BusinessIntelligence.Infrastructure.Persistence;
using FusionOS.Modules.BusinessIntelligence.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.BusinessIntelligence.Api;

/// <summary>
/// Phase 6 — Business Intelligence, first slice (2026-07-18). Registers the module's
/// DbContext, KpiDefinitions + KpiSnapshots CQRS, repositories, and the outbox
/// dispatcher that relays KpiDefinitionCreated/KpiSnapshotRecorded to Kafka
/// (03_SYSTEM_ARCHITECTURE.md §4.2). BI publishes events but consumes none this
/// slice, so no IIntegrationEventConsumer is registered here — consistent with
/// this codebase's governing principle that BI must never be a synchronous
/// dependency of a transactional module.
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

        services.AddScoped<IKpiDefinitionRepository, KpiDefinitionRepository>();
        services.AddScoped<IKpiSnapshotRepository, KpiSnapshotRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateKpiDefinitionCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(BusinessIntelligenceModule).Assembly);

        services.AddHostedService<OutboxDispatcher<BusinessIntelligenceDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
