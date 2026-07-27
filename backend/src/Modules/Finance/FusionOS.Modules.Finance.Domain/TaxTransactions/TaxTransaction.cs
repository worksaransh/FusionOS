using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.TaxTransactions;

/// <summary>
/// GST filing support (2026-07-22). The real, honestly-documented reason
/// this exists: Finance's tax reporting could not previously see any
/// per-transaction tax amount, because Sales' InvoiceIssued and Procurement's
/// PurchaseOrderGoodsReceiptCosted integration events never carried
/// TaxRateId/TaxAmount (confirmed by a prior audit pass on this codebase).
/// This entity is the landing spot for that data once those two events are
/// additively extended to include it — one row per taxed line, recorded by
/// Finance's own consumers at the moment the event arrives, so
/// GetGstReturnSummaryReportQuery has real numbers to group instead of
/// nullable placeholders.
///
/// Direction is Output (a sale — the company collected this tax and owes it
/// to the government) or Input (a purchase — the company paid this tax and
/// may claim it as input tax credit). TaxRateId is a same-module FK (Finance
/// owns TaxRate); SourceEntityType/SourceEntityId is the opaque
/// (EntityType, EntityId) cross-module reference into whichever Invoice or
/// PurchaseOrder line this came from — same polymorphic-reference convention
/// as Core's ApprovalRequest, since Finance cannot take a compile-time
/// dependency on Sales/Procurement's domain types.
/// </summary>
public sealed class TaxTransaction : TenantAggregateRoot
{
    public TaxTransactionDirection Direction { get; private set; }
    public Guid TaxRateId { get; private set; }
    public decimal TaxAmount { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public string SourceEntityType { get; private set; } = default!;
    public Guid SourceEntityId { get; private set; }

    private TaxTransaction() { }

    public static TaxTransaction Record(
        Guid companyId,
        TaxTransactionDirection direction,
        Guid taxRateId,
        decimal taxAmount,
        DateTimeOffset transactionDate,
        string sourceEntityType,
        Guid sourceEntityId)
    {
        if (taxRateId == Guid.Empty)
            throw new ArgumentException("Tax rate id is required.", nameof(taxRateId));
        if (taxAmount < 0)
            throw new ArgumentException("Tax amount cannot be negative.", nameof(taxAmount));
        if (string.IsNullOrWhiteSpace(sourceEntityType))
            throw new ArgumentException("Source entity type is required.", nameof(sourceEntityType));
        if (sourceEntityId == Guid.Empty)
            throw new ArgumentException("Source entity id is required.", nameof(sourceEntityId));

        return new TaxTransaction
        {
            CompanyId = companyId,
            Direction = direction,
            TaxRateId = taxRateId,
            TaxAmount = taxAmount,
            TransactionDate = transactionDate,
            SourceEntityType = sourceEntityType.Trim(),
            SourceEntityId = sourceEntityId,
        };
    }
}
