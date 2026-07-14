namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum PurchaseOrderStatus
{
    Draft,
    Approved,

    /// <summary>At least one line has a non-zero ReceivedQuantity but not every line is fully received yet.</summary>
    PartiallyReceived,

    /// <summary>Every line's ReceivedQuantity has reached (or exceeded) its ordered Quantity.</summary>
    FullyReceived,
}
