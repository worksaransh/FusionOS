using FusionOS.Modules.Inventory.Domain.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantAttributeValueConfiguration : IEntityTypeConfiguration<ProductVariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttributeValue> builder)
    {
        builder.ToTable("product_variant_attribute_values");
        builder.HasKey(x => x.Id);
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        // One value per definition per variant — see ProductVariantAttributeValue's/
        // AssignAttributeValueToVariantCommandHandler's doc comments (upsert semantics).
        builder.HasIndex(x => new { x.VariantId, x.AttributeDefinitionId }).IsUnique();
        builder.HasIndex(x => x.ProductId);
        builder.Ignore(x => x.DomainEvents);
    }
}
