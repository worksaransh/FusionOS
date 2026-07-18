namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders;

/// <summary>
/// A component requirement snapshotted onto a <see cref="WorkOrder"/> at creation time:
/// the total quantity of one Inventory Product this whole work order will consume
/// (BOM per-unit quantity × the order's QuantityToProduce). Snapshotted rather than
/// re-read from the BillOfMaterials at completion so a later edit to the BOM never
/// retroactively changes what an in-flight or completed work order consumed — the same
/// "capture the number at the point of the transaction" discipline the ledger uses.
/// Same documented no-audit/tenant-columns exception as every other line entity.
/// </summary>
public sealed class WorkOrderComponent
{
    public Guid Id { get; private set; }
    public Guid ComponentProductId { get; private set; }
    public decimal QuantityRequired { get; private set; }

    private WorkOrderComponent() { }

    internal static WorkOrderComponent Create(Guid componentProductId, decimal quantityRequired)
    {
        if (componentProductId == Guid.Empty)
            throw new ArgumentException("Component product id is required.", nameof(componentProductId));
        if (quantityRequired <= 0)
            throw new ArgumentException("Required quantity must be greater than zero.", nameof(quantityRequired));

        return new WorkOrderComponent
        {
            Id = Guid.NewGuid(),
            ComponentProductId = componentProductId,
            QuantityRequired = quantityRequired,
        };
    }
}
