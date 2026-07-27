using FusionOS.Modules.Inventory.Domain.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("attribute_values");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasMaxLength(100).IsRequired();
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        // Unique per definition, not per company — see AttributeValue's doc comment.
        builder.HasIndex(x => new { x.AttributeDefinitionId, x.Value }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
