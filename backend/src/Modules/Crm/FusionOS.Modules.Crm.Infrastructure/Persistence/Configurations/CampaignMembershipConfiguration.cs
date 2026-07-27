using FusionOS.Modules.Crm.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Crm.Infrastructure.Persistence.Configurations;

public sealed class CampaignMembershipConfiguration : IEntityTypeConfiguration<CampaignMembership>
{
    public void Configure(EntityTypeBuilder<CampaignMembership> builder)
    {
        builder.ToTable("campaign_memberships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.CampaignId);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.CampaignId, x.EntityType, x.EntityId }).IsUnique();
    }
}
