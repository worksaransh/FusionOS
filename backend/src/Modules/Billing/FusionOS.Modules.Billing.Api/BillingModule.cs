using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Billing.Application.Bootstrap;
using FusionOS.Modules.Billing.Application.Entitlements;
using FusionOS.Modules.Billing.Application.Plans.Commands.CreatePlan;
using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Application.UsageRecords.Contracts;
using FusionOS.Modules.Billing.Infrastructure.Persistence;
using FusionOS.Modules.Billing.Infrastructure.Repositories;
using FusionOS.SharedKernel.Events;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Billing.Api;

/// <summary>
/// Phase 1.3 — Billing/subscription domain. Plan (global pricing catalog: Starter/Growth/
/// Enterprise), Subscription (a company's Trial/Active/PastDue/Cancelled subscription to a
/// Plan), and UsageRecord (an append-only metering trail against a Subscription) are the
/// first slice — the literal prerequisite before FusionOS's own SaaS pricing tiers can mean
/// anything, since there was previously no way to meter or bill a tenant for usage. No payment
/// gateway integration and no real money movement in this pass; see each aggregate's own class
/// doc comment for the exact scope line.
///
/// IBillingBootstrapService (registered below) is the cross-module hook Core's
/// RegisterUserCommandHandler calls to give a brand-new company's bootstrap a default trial
/// Subscription automatically — see BillingBootstrapService's own doc comment.
/// </summary>
public sealed class BillingModule : IModule
{
    public string Name => "Billing";
    public string RoadmapPhase => "Phase 1.3 — Billing/Subscription Domain";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BillingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "billing")));

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUsageRecordRepository, UsageRecordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<BillingDbContext>>();

        // Cross-module bootstrap hook — see BillingModule's own class doc comment and
        // IBillingBootstrapService's doc comment (FusionOS.BuildingBlocks.Application.Abstractions)
        // for why this is published through the BuildingBlocks abstraction rather than a direct
        // Core -> Billing project reference.
        services.AddScoped<IBillingBootstrapService, BillingBootstrapService>();

        // Cross-module entitlement hook — see IPlanEntitlementService's doc comment
        // (FusionOS.BuildingBlocks.Application.Abstractions) and PlanEntitlementBehavior
        // (FusionOS.BuildingBlocks.Application.Behaviors), which calls this on (almost) every
        // request in the MediatR pipeline.
        services.AddScoped<IPlanEntitlementService, PlanEntitlementService>();

        services.AddModuleApplication(typeof(CreatePlanCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(BillingModule).Assembly);

        services.AddHostedService<OutboxDispatcher<BillingDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
