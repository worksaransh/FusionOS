using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using FusionOS.Modules.Sales.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Sales.Api;

public sealed class SalesModule : IModule
{
    public string Name => "Sales";
    public string RoadmapPhase => "Phase 1 — Trading ERP Core";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "sales")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IDispatchRepository, DispatchRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateCustomerCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(SalesModule).Assembly);

        services.AddHostedService<OutboxDispatcher<SalesDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
