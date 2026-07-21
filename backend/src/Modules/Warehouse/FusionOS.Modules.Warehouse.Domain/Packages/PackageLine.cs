namespace FusionOS.Modules.Warehouse.Domain.Packages;

/// <summary>
/// Plain child class (not TenantAggregateRoot), same shape as PickListLine — private setters, an
/// `internal static Create`, only ever constructed by the parent Package aggregate. Unlike
/// PickListLine (which needs RecordPicked because a pick list is progressively fulfilled),
/// PackageLine is immutable once created: a Package is a record of what was actually put into a
/// carton at packing time, not a document that changes afterward, so there's no in-place mutator
/// here — same immutability convention as GoodsReceiptLine's core fields / PurchaseOrderLine.
/// </summary>
public sealed class PackageLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }

    private PackageLine() { }

    internal static PackageLine Create(Guid productId, decimal quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new PackageLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
        };
    }
}
