using FusionOS.SharedKernel;
using FusionOS.Modules.Inventory.Domain.Reservations.Events;

namespace FusionOS.Modules.Inventory.Domain.Reservations;

/// <summary>
/// Phase 1 closeout (2026-07-18): soft-holds a quantity of a Product at a
/// Warehouse against a reference document (e.g. a Sales Order line) before
/// it's actually picked/dispatched — 05_MODULE_ROADMAP.md's Inventory
/// "Reservations" line item, confirmed absent by a repo-wide grep before this
/// slice. Active → Released (the hold is cancelled, e.g. the order line was
/// cancelled) or Fulfilled (the hold converted into a real stock movement).
///
/// This does NOT itself post an InventoryLedgerEntry — a reservation is a
/// soft hold, not a movement; the actual ledger entry is still posted by the
/// existing Dispatch-consuming path (DispatchLineDispatchedConsumer) exactly
/// as before this slice. Available-to-promise (GetAvailableToPromiseQuery,
/// same module) is StockOnHand minus the sum of Active reservations —
/// callers combine the two rather than this aggregate trying to own both.
///
/// ReferenceType/ReferenceId are an opaque cross-module reference (e.g.
/// ReferenceType="SalesOrderLine"), same convention as InventoryLedgerEntry's
/// own WarehouseId — never existence-validated here, no foreign key across
/// module boundaries.
/// </summary>
public sealed class Reservation : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal Quantity { get; private set; }
    public string ReferenceType { get; private set; } = default!;
    public Guid ReferenceId { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    public static Reservation Create(Guid companyId, Guid productId, Guid warehouseId, decimal quantity, string referenceType, Guid referenceId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be greater than zero.", nameof(quantity));
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("Reference type is required.", nameof(referenceType));
        if (referenceId == Guid.Empty)
            throw new ArgumentException("Reference id is required.", nameof(referenceId));

        var reservation = new Reservation
        {
            CompanyId = companyId,
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            ReferenceType = referenceType.Trim(),
            ReferenceId = referenceId,
            Status = ReservationStatus.Active,
        };

        reservation.Raise(new ReservationCreated(reservation.Id, companyId, productId, warehouseId, quantity));
        return reservation;
    }

    /// <summary>Cancels the hold without a stock movement — e.g. the referencing Sales Order line was cancelled before dispatch.</summary>
    public void Release()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Only an Active reservation can be released (current status: {Status}).");

        Status = ReservationStatus.Released;
    }

    /// <summary>Marks the hold as converted into a real stock movement — the caller (e.g. a Dispatch confirmation flow) is responsible for the actual ledger entry.</summary>
    public void Fulfill()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Only an Active reservation can be fulfilled (current status: {Status}).");

        Status = ReservationStatus.Fulfilled;
        Raise(new ReservationFulfilled(Id, CompanyId, ProductId, WarehouseId, Quantity));
    }
}
