using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Payables.Contracts;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordBillCharge;

/// <summary>
/// Records that FusionOS's company owes a supplier money for a bill — the
/// manual-entry charge side of the AP ledger (Phase M8c, 2026-07-17; see
/// ApLedgerEntry's class doc comment for why this is a manual command rather
/// than a Kafka-consumer reaction to a PurchaseOrder/GoodsReceipt event).
/// PurchaseOrderId is optional — an ad-hoc supplier bill has no PO at all.
/// SupplierId and PurchaseOrderId are opaque references into Procurement,
/// same as ArLedgerEntry's CustomerId/InvoiceId references into Sales — this
/// handler does not call into Procurement to verify them.
/// </summary>
public sealed record RecordBillChargeCommand(Guid CompanyId, Guid SupplierId, Guid? PurchaseOrderId, decimal Amount, string Description)
    : ICommand<ApLedgerEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.payable.record-charge" };
    public string EntityType => nameof(Domain.Payables.ApLedgerEntry);
    public Guid EntityId { get; init; }
    public string Action => "Recorded bill charge";
}
