using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;

namespace FusionOS.Modules.Finance.Application.Receivables.Commands.RecordPayment;

/// <summary>
/// Records a customer payment against a specific invoice, reducing that
/// invoice's outstanding AR balance (Phase M4, 2026-07-15 — the AR ledger
/// previously only ever increased via InvoiceIssuedConsumer). CustomerId and
/// InvoiceId are opaque references into Sales, same as RecordInvoiceCharge —
/// this handler does not call into the Sales module to verify them, matching
/// the documented cross-module-reference pattern on ArLedgerEntry itself.
/// </summary>
public sealed record RecordPaymentCommand(Guid CompanyId, Guid CustomerId, Guid InvoiceId, decimal Amount, DateTimeOffset? PaymentDate, string? Reference)
    : ICommand<ArLedgerEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.receivable.record-payment" };
    public string EntityType => nameof(Domain.Receivables.ArLedgerEntry);
    public Guid EntityId { get; init; }
    public string Action => "Recorded payment";
}
