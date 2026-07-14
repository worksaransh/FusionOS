using FusionOS.Modules.Core.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(n => n.RowVersion);
        builder.HasIndex(n => new { n.CompanyId, n.RecipientUserId, n.IsRead });
        builder.Ignore(n => n.DomainEvents);
    }
}
