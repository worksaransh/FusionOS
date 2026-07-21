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

    /// <summary>How much of this component has actually been issued (picked/consumed) to the shop floor so far — distinct from QuantityRequired, the planned snapshot. Never exceeds QuantityRequired.</summary>
    public decimal QuantityIssued { get; private set; }

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

    /// <summary>Records that <paramref name="quantity"/> more of this component has been issued. Cannot push the running total past QuantityRequired.</summary>
    internal void Issue(decimal quantity)
    {
        var newTotal = QuantityIssued + quantity;
        if (newTotal > QuantityRequired)
        {
            throw new InvalidOperationException(
                $"Cannot issue {quantity} of component '{ComponentProductId}': only {QuantityRequired - QuantityIssued} remains unissued.");
        }

        QuantityIssued = newTotal;
    }

    /// <summary>Reverses a prior issuance — the inverse of <see cref="Issue"/>. Cannot return more than has actually been issued.</summary>
    internal void Return(decimal quantity)
    {
        if (quantity > QuantityIssued)
        {
            throw new InvalidOperationException(
                $"Cannot return {quantity} of component '{ComponentProductId}': only {QuantityIssued} has been issued.");
        }

        QuantityIssued -= quantity;
    }
}
