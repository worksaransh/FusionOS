using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
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
/// InvoiceIssued event (05_MODULE_ROADMAP.md). Cost centers (M8a), the
/// multi-jurisdiction tax engine's TaxJurisdiction/TaxRate master data (M8b),
/// a minimal Accounts Payable ledger (M8c — manual RecordBillChargeCommand
/// plus, as of the Step-2 integration-gap pass, an automatic charge fed by
/// Procurement's PurchaseOrderGoodsReceiptCosted event when a receipt carries
/// a real UnitCost; see ApLedgerEntry's class doc comment), and bank reconciliation's
/// BankAccount/BankStatementLine master data (M8d — manual entry and manual
/// reconcile/unreconcile only, no bank-feed import and no auto-matching; see
/// BankStatementLine's class doc comment), ExchangeRate master data plus
/// a conversion query (M8e — dated FX rates and ConvertAmountQuery only, no
/// existing aggregate is currency-aware yet; see ExchangeRate's class doc
/// comment), and Budget/BudgetLine master data plus a read-only actual-vs-
/// budget query (M8f — see Budget's and GetBudgetVsActualQueryHandler's
/// class doc comments for the scope line and the cost-center-level-actuals
/// limitation), and FixedAsset master data plus a pure on-demand
/// straight-line depreciation calculation query (M8g — no automated
/// depreciation-posting run, no disposal gain/loss GL posting; see
/// FixedAsset's and GetDepreciationScheduleQueryHandler's own class doc
/// comments) are now live.
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
        services.AddScoped<IApLedgerRepository, ApLedgerRepository>();
        services.AddScoped<ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<ITaxJurisdictionRepository, TaxJurisdictionRepository>();
        services.AddScoped<ITaxRateRepository, TaxRateRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankStatementLineRepository, BankStatementLineRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IBudgetLineRepository, BudgetLineRepository>();
        services.AddScoped<IFixedAssetRepository, FixedAssetRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<FinanceDbContext>>();

        services.AddModuleApplication(typeof(CreateAccountCommand).Assembly);

        // Kafka consumer side of 03_SYSTEM_ARCHITECTURE.md §4.2 — Finance reacts
        // to Sales' issued invoices by posting an Accounts Receivable charge.
        // Discovered generically by KafkaConsumerHostedService at startup.
        services.AddScoped<IIntegrationEventConsumer, InvoiceIssuedConsumer>();

        // Finance reacts to Sales' issued credit notes by posting a negative
        // Accounts Receivable entry — mirrors InvoiceIssuedConsumer's registration.
        services.AddScoped<IIntegrationEventConsumer, CreditNoteIssuedConsumer>();

        // Finance reacts to Procurement's costed goods receipts by posting an
        // Accounts Payable charge automatically — closes the auto-charge
        // blocker documented on ApLedgerEntry (Phase M8c / Step-2 integration gaps).
        services.AddScoped<IIntegrationEventConsumer, PurchaseOrderGoodsReceiptCostedConsumer>();

        services.AddControllers().AddApplicationPart(typeof(FinanceModule).Assembly);

        services.AddHostedService<OutboxDispatcher<FinanceDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
