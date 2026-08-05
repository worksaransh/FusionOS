using FusionOS.Modules.Core.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.TaxId).HasMaxLength(50);
        builder.Property(c => c.BaseCurrency).HasMaxLength(3).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(c => c.RowVersion);
        builder.HasIndex(c => c.Name);
        builder.Ignore(c => c.DomainEvents);

        // Self-referencing parent/subsidiary link (Phase 2.4 — multi-entity consolidation).
        // Nullable, no cascade delete: deleting/deactivating a parent must never cascade
        // into its subsidiaries' rows — Company is never hard-deleted anyway (soft-delete
        // only), but Restrict is the safe default for a self-FK regardless.
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(c => c.ParentCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => c.ParentCompanyId);
    }
}
