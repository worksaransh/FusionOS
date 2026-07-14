using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using FusionOS.Modules.Finance.Infrastructure.Repositories;
using FusionOS.SharedKernel.Events;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Finance.Api;

/// <summary>
/// Phase 2 — Financial Backbone. Chart of Accounts, General Ledger journal
/// entries, and a minimal Accounts Receivable ledger fed by Sales'
/// InvoiceIssued event (05_MODULE_ROADMAP.md). AP, GST/Taxes, cost centers,
/// and bank reconciliation are still reserved for later slices.
/// </summary>
public sealed class FinanceModule : IModule
{
    public string Name => "Finance";
    public string RoadmapPhase => "Phase 2 — Financial Backbone";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FinanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "finance")));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IArLedgerRepository, ArLedgerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<FinanceDbContext>>();

        services.AddModuleApplication(typeof(CreateAccountCommand).Assembly);

        // Kafka consumer side of 03_SYSTEM_ARCHITECTURE.md §4.2 — Finance reacts
        // to Sales' issued invoices by posting an Accounts Receivable charge.
        // Discovered generically by KafkaConsumerHostedService at startup.
        services.AddScoped<IIntegrationEventConsumer, InvoiceIssuedConsumer>();

        services.AddControllers().AddApplicationPart(typeof(FinanceModule).Assembly);

        services.AddHostedService<OutboxDispatcher<FinanceDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
