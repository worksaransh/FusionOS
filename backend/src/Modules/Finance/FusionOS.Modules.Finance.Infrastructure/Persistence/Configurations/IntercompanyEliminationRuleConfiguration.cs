using FusionOS.Modules.Finance.Domain.IntercompanyEliminationRules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class IntercompanyEliminationRuleConfiguration : IEntityTypeConfiguration<IntercompanyEliminationRule>
{
    public void Configure(EntityTypeBuilder<IntercompanyEliminationRule> builder)
    {
        builder.ToTable("intercompany_elimination_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReceivableAccountCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PayableAccountCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.ReceivableAccountCode, x.PayableAccountCode }).IsUnique();
    }
}
