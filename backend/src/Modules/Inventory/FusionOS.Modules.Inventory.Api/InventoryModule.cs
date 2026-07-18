using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Inventory.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using FusionOS.Modules.Inventory.Infrastructure.Repositories;
using FusionOS.SharedKernel.Events;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Inventory.Api;

/// <summary>Inventory's Product and Stock Ledger slices are registered here (03_SYSTEM_ARCHITECTURE.md).</summary>
public sealed class InventoryModule : IModule
{
    public string Name => "Inventory";
    public string RoadmapPhase => "Phase 1 — Trading ERP Core";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "inventory")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryLedgerRepository, InventoryLedgerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<InventoryDbContext>>();

        services.AddModuleApplication(typeof(CreateProductCommand).Assembly);

        // Kafka consumer side of 03_SYSTEM_ARCHITECTURE.md §4.2 — Inventory reacts
        // to cross-module events raised by Warehouse (goods receipts) and Sales
        // (dispatches) by keeping its own Stock Ledger in sync. Discovered
        // generically by KafkaConsumerHostedService at startup.
        services.AddScoped<IIntegrationEventConsumer, GoodsReceiptLineReceivedConsumer>();
        services.AddScoped<IIntegrationEventConsumer, DispatchLineDispatchedConsumer>();
        services.AddScoped<IIntegrationEventConsumer, CycleCountVarianceRecordedConsumer>();
        // Manufacturing's completed work orders move stock too: consume components,
        // produce the parent product (03_SYSTEM_ARCHITECTURE.md §4.2).
        services.AddScoped<IIntegrationEventConsumer, WorkOrderCompletedConsumer>();

        services.AddControllers().AddApplicationPart(typeof(InventoryModule).Assembly);

        services.AddHostedService<OutboxDispatcher<InventoryDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
