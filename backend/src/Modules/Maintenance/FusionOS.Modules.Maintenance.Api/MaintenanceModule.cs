using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Maintenance.Application.Assets.Commands.CreateAsset;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using FusionOS.Modules.Maintenance.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Maintenance.Api;

/// <summary>
/// Phase 5 — Maintenance, first slice (2026-07-18). Registers the module's DbContext,
/// Assets + MaintenanceRequests CQRS, repositories, and the outbox dispatcher that
/// relays AssetCreated/MaintenanceRequestCreated/MaintenanceRequestCompleted to Kafka
/// (03_SYSTEM_ARCHITECTURE.md §4.2). Maintenance publishes events but consumes none,
/// so no IIntegrationEventConsumer is registered here.
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

        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IMaintenanceRequestRepository, MaintenanceRequestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateAssetCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(MaintenanceModule).Assembly);

        services.AddHostedService<OutboxDispatcher<MaintenanceDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
