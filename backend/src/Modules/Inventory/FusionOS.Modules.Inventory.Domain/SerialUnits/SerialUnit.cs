using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.SerialUnits.Events;

namespace FusionOS.Modules.Inventory.Domain.SerialUnits;

/// <summary>
/// Structured serial-number tracking for a Product — sits alongside the
/// existing opaque InventoryLedgerEntry.SerialNumber/GoodsReceiptLine.SerialNumber
/// string fields (Warehouse module) without touching them: those already flow
/// through GoodsReceiptLineReceivedConsumer into the ledger today and stay
/// exactly as they are. This aggregate is the real "what do we know about
/// unit S/N 12345" record — the actual "scan a serial and find it" use case
/// (GetSerialUnitBySerialNumberQuery) that an opaque ledger string alone can't
/// answer.
///
/// ProductId is a real, same-module foreign key (Product lives in this
/// module), validated by the command handler via IProductRepository — same
/// convention as Batch.ProductId (this module) / CreateMaintenanceRequest's
/// AssetId (Maintenance).
///
/// Status is a small state machine, same style as MaintenanceRequest's
/// Open -> InProgress -> Completed: InStock is the only starting state (set
/// at registration); MarkReserved/MarkSold/MarkReturned/MarkDefective each
/// validate the transition is legal from the unit's current status rather
/// than blindly overwriting it. A Sold unit must be Returned before it can be
/// marked Defective — discovering a defect on an already-sold, not-yet-
/// returned unit is out of scope for this slice (it would need a real
/// returns/RMA workflow, not just a status flag).
/// </summary>
public sealed class SerialUnit : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public string SerialNumber { get; private set; } = default!;
    public SerialUnitStatus Status { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }

    private SerialUnit() { }

    public static SerialUnit Create(Guid companyId, Guid productId, string serialNumber)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number is required.", nameof(serialNumber));

        var unit = new SerialUnit
        {
            CompanyId = companyId,
            ProductId = productId,
            SerialNumber = serialNumber.Trim(),
            Status = SerialUnitStatus.InStock,
            ReceivedAt = DateTimeOffset.UtcNow,
        };

        unit.Raise(new SerialUnitRegistered(unit.Id, companyId, productId, unit.SerialNumber));
        return unit;
    }

    /// <summary>Soft-holds this unit against a pending sale. Requires InStock — a Reserved/Sold/Returned/Defective unit is not available to reserve again.</summary>
    public void MarkReserved()
    {
        if (Status != SerialUnitStatus.InStock)
            throw new InvalidOperationException($"Only an InStock serial unit can be reserved (current status: {Status}).");

        Status = SerialUnitStatus.Reserved;
    }

    /// <summary>Marks the unit sold. Allowed from InStock (a direct sale with no prior reservation) or Reserved (the common path). Cannot sell an already-sold unit, nor one that's Returned/Defective.</summary>
    public void MarkSold()
    {
        if (Status is not (SerialUnitStatus.InStock or SerialUnitStatus.Reserved))
            throw new InvalidOperationException($"Only an InStock or Reserved serial unit can be sold (current status: {Status}).");

        Status = SerialUnitStatus.Sold;
    }

    /// <summary>Records a customer return. Requires Sold — a unit that was never sold cannot be "returned".</summary>
    public void MarkReturned()
    {
        if (Status != SerialUnitStatus.Sold)
            throw new InvalidOperationException($"Only a Sold serial unit can be returned (current status: {Status}).");

        Status = SerialUnitStatus.Returned;
    }

    /// <summary>Flags the unit as defective/unsellable. Allowed from any status except Sold — see class doc comment for why a Sold unit must be Returned first.</summary>
    public void MarkDefective()
    {
        if (Status == SerialUnitStatus.Sold)
            throw new InvalidOperationException("A Sold serial unit cannot be marked defective directly — return it first.");

        Status = SerialUnitStatus.Defective;
    }
}
