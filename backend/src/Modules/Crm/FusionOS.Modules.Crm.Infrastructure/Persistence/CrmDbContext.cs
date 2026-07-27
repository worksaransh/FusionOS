using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Persistence;

/// <summary>
/// Owns the "crm" schema. Phase 4 — CRM: Leads, Opportunities, Accounts, Contacts, and
/// Activities. Adding an entity here also means adding its IEntityTypeConfiguration and an
/// EF Core migration, per docs/blueprint/04_DATABASE_GUIDELINES.md §9.
/// </summary>
public sealed class CrmDbContext : BaseDbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<FusionOS.Modules.Crm.Domain.Leads.Lead> Leads => Set<FusionOS.Modules.Crm.Domain.Leads.Lead>();
    public DbSet<FusionOS.Modules.Crm.Domain.Opportunities.Opportunity> Opportunities => Set<FusionOS.Modules.Crm.Domain.Opportunities.Opportunity>();
    public DbSet<FusionOS.Modules.Crm.Domain.Accounts.Account> Accounts => Set<FusionOS.Modules.Crm.Domain.Accounts.Account>();
    public DbSet<FusionOS.Modules.Crm.Domain.Contacts.Contact> Contacts => Set<FusionOS.Modules.Crm.Domain.Contacts.Contact>();
    public DbSet<FusionOS.Modules.Crm.Domain.Activities.Activity> Activities => Set<FusionOS.Modules.Crm.Domain.Activities.Activity>();
    public DbSet<FusionOS.Modules.Crm.Domain.Tasks.FollowUpTask> FollowUpTasks => Set<FusionOS.Modules.Crm.Domain.Tasks.FollowUpTask>();
    public DbSet<FusionOS.Modules.Crm.Domain.Campaigns.Campaign> Campaigns => Set<FusionOS.Modules.Crm.Domain.Campaigns.Campaign>();
    public DbSet<FusionOS.Modules.Crm.Domain.Campaigns.CampaignMembership> CampaignMemberships => Set<FusionOS.Modules.Crm.Domain.Campaigns.CampaignMembership>();
    public DbSet<FusionOS.Modules.Crm.Domain.Tags.Tag> Tags => Set<FusionOS.Modules.Crm.Domain.Tags.Tag>();
    public DbSet<FusionOS.Modules.Crm.Domain.Tags.EntityTag> EntityTags => Set<FusionOS.Modules.Crm.Domain.Tags.EntityTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
