using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Sales.Application.IntegrationEvents.Consumers;
using FusionOS.SharedKernel.Events;
using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using FusionOS.Modules.Sales.Infrastructure.Repositories;
using FusionOS.Modules.Sales.Infrastructure.Security;
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
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<IPriceListRepository, PriceListRepository>();
        services.AddScoped<ISalesCommissionRateRepository, SalesCommissionRateRepository>();
        services.AddScoped<IDiscountRuleRepository, DiscountRuleRepository>();

        // Sales Returns / RMA (Phase 4 depth, 2026-07-22) — distinct from and
        // optionally linked to CreditNote, never auto-cascading into one.
        services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();

        // Customer Portal MVP (Phase 4, net-new, 2026-07-22) — a completely
        // separate identity system from staff Users/RBAC; see
        // CustomerPortalUser's own doc comment for why.
        services.AddScoped<ICustomerPortalUserRepository, CustomerPortalUserRepository>();
        services.AddScoped<IPortalPasswordHasher, BCryptPortalPasswordHasher>();
        services.AddScoped<IPortalTokenService, PortalJwtTokenService>();
        services.Configure<PortalJwtOptions>(configuration.GetSection(PortalJwtOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateCustomerCommand).Assembly);

        // Kafka consumer side (03_SYSTEM_ARCHITECTURE.md §4.2): Sales reacts to CRM's
        // OpportunityWon by creating the won deal's Customer. Discovered generically by
        // KafkaConsumerHostedService at startup.
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<SalesDbContext>>();
        services.AddScoped<IIntegrationEventConsumer, OpportunityWonConsumer>();

        services.AddControllers().AddApplicationPart(typeof(SalesModule).Assembly);

        services.AddHostedService<OutboxDispatcher<SalesDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
