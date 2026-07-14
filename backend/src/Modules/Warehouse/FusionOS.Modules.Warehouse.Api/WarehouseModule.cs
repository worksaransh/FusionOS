using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.CreateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using FusionOS.Modules.Warehouse.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Warehouse.Api;

public sealed class WarehouseModule : IModule
{
    public string Name => "Warehouse";
    public string RoadmapPhase => "Phase 1 — Trading ERP Core";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WarehouseDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "warehouse")));

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IZoneRepository, ZoneRepository>();
        services.AddScoped<IGoodsReceiptRepository, GoodsReceiptRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateWarehouseCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(WarehouseModule).Assembly);

        services.AddHostedService<OutboxDispatcher<WarehouseDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
