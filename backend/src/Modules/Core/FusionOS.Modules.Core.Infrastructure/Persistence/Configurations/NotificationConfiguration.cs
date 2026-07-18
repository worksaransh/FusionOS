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
        builder.Property(n => n.DeliveryStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.DeliveryError).HasMaxLength(2000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(n => n.RowVersion);
        builder.HasIndex(n => new { n.CompanyId, n.RecipientUserId, n.IsRead });

        // Backs NotificationDeliveryDispatcher's cross-tenant poll for anything
        // not yet Sent — deliberately not scoped to CompanyId (see
        // INotificationRepository.GetPendingDeliveryAsync's doc comment).
        builder.HasIndex(n => n.DeliveryStatus);

        builder.Ignore(n => n.DomainEvents);
    }
}
