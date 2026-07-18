using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Procurement.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using FusionOS.Modules.Procurement.Infrastructure.Repositories;
using FusionOS.SharedKernel.Events;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Procurement.Api;

public sealed class ProcurementModule : IModule
{
    public string Name => "Procurement";
    public string RoadmapPhase => "Phase 1 — Trading ERP Core";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProcurementDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "procurement")));

        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IRfqRepository, RfqRepository>();
        services.AddScoped<ISupplierContractRepository, SupplierContractRepository>();
        services.AddScoped<IVendorReturnRepository, VendorReturnRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<ProcurementDbContext>>();

        services.AddModuleApplication(typeof(CreateSupplierCommand).Assembly);

        // Kafka consumer side of 03_SYSTEM_ARCHITECTURE.md §4.2 — Procurement
        // reacts to Warehouse's goods receipts by advancing the matching
        // Purchase Order's received-status. Discovered generically by
        // KafkaConsumerHostedService at startup.
        services.AddScoped<IIntegrationEventConsumer, GoodsReceiptLineReceivedConsumer>();

        services.AddControllers().AddApplicationPart(typeof(ProcurementModule).Assembly);

        services.AddHostedService<OutboxDispatcher<ProcurementDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
