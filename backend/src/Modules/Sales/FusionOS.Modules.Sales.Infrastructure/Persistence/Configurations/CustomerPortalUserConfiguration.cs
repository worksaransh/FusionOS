using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CustomerPortalUserConfiguration : IEntityTypeConfiguration<Domain.PortalUsers.CustomerPortalUser>
{
    public void Configure(EntityTypeBuilder<Domain.PortalUsers.CustomerPortalUser> builder)
    {
        builder.ToTable("customer_portal_users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);

        // Email is unique per-Customer, NOT globally - see CustomerPortalUser's
        // own doc comment for why. PortalLoginCommandHandler's GetByEmailAsync
        // therefore returns a list, never a single row.
        builder.HasIndex(x => new { x.CustomerId, x.Email }).IsUnique();
        builder.HasIndex(x => x.Email); // login lookup by email alone, across customers
        builder.Ignore(x => x.DomainEvents);
    }
}
