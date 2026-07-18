using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using FusionOS.Modules.Manufacturing.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Manufacturing.Api;

/// <summary>
/// Phase 3 — Manufacturing ERP, first slice. Registers the module's DbContext,
/// Bills of Materials + Work Orders CQRS, repositories, and the outbox dispatcher
/// that relays WorkOrderCompleted to Kafka for Inventory to consume
/// (03_SYSTEM_ARCHITECTURE.md §4.2). Manufacturing publishes events but consumes
/// none, so no IIntegrationEventConsumer is registered here.
/// </summary>
public sealed class ManufacturingModule : IModule
{
    public string Name => "Manufacturing";
    public string RoadmapPhase => "Phase 3 — Manufacturing ERP";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ManufacturingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "manufacturing")));

        services.AddScoped<IBillOfMaterialsRepository, BillOfMaterialsRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateBillOfMaterialsCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(ManufacturingModule).Assembly);

        services.AddHostedService<OutboxDispatcher<ManufacturingDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
