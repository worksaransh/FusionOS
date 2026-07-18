using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.IntegrationHub.Api;

/// <summary>
/// Phase 9 — Integration Hub, first slice (2026-07-18). Registers the module's DbContext,
/// IntegrationConnectors + ConnectorConnections CQRS, repositories, and the outbox
/// dispatcher that relays IntegrationConnectorCreated/ConnectorConnected to Kafka
/// (03_SYSTEM_ARCHITECTURE.md §4.2). IntegrationHub publishes events but consumes none
/// this slice, so no IIntegrationEventConsumer is registered here.
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

        services.AddScoped<IIntegrationConnectorRepository, IntegrationConnectorRepository>();
        services.AddScoped<IConnectorConnectionRepository, ConnectorConnectionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateIntegrationConnectorCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(IntegrationHubModule).Assembly);

        services.AddHostedService<OutboxDispatcher<IntegrationHubDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
