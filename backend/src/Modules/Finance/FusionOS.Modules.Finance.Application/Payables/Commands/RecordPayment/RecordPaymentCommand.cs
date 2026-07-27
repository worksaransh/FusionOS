using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Payables.Contracts;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordPayment;

/// <summary>
/// Records a payment FusionOS's company makes to a supplier, reducing that
/// supplier's outstanding AP balance — mirrors
/// Receivables.Commands.RecordPayment.RecordPaymentCommand, except scoped to
/// the supplier's total balance rather than one specific invoice/PO (see
/// ApLedgerEntry's class doc comment for why). SupplierId and
/// PurchaseOrderId are opaque references into Procurement, same as
/// RecordBillChargeCommand — this handler does not call into Procurement to
/// verify them.
///
/// BankAccountId is a new, optional trailing field (bank-tied-payments
/// follow-up) — which bank account the payment actually moved through, an
/// opaque reference into Finance's own BankAccount aggregate, not verified
/// here either (see ApLedgerEntry.BankAccountId's own doc comment). Added at
/// the end so every existing call site keeps compiling unchanged.
/// </summary>
public sealed record RecordPaymentCommand(Guid CompanyId, Guid SupplierId, Guid? PurchaseOrderId, decimal Amount, DateTimeOffset? PaymentDate, string? Reference, Guid? BankAccountId = null)
    : ICommand<ApLedgerEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.payable.record-payment" };
    public string EntityType => nameof(Domain.Payables.ApLedgerEntry);
    public Guid EntityId { get; init; }
    public string Action => "Recorded payment";
}
