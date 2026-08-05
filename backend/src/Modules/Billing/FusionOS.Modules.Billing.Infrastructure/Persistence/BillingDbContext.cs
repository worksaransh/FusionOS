using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Billing.Domain.Plans;
using FusionOS.Modules.Billing.Domain.Subscriptions;
using FusionOS.Modules.Billing.Domain.UsageRecords;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Billing.Infrastructure.Persistence;

/// <summary>
/// Owns the "billing" schema (Phase 1.3 — the prerequisite before FusionOS's own SaaS pricing
/// tiers can mean anything, since there was previously no way to meter or bill a tenant for
/// usage). Plan (global pricing catalog), Subscription (a company's subscription to a Plan,
/// with a Trial/Active/PastDue/Cancelled state machine), and UsageRecord (an append-only
/// metering trail against a Subscription) are the first slice — no payment gateway
/// integration, no real money movement.
/// </summary>
public sealed class BillingDbContext : BaseDbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
