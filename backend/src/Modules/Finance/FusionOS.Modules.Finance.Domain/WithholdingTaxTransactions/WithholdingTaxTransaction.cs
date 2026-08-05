using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.WithholdingTaxTransactions;

/// <summary>
/// Append-only audit trail of a real TDS/TCS deduction — created once by
/// RecordWithholdingTaxDeductionCommandHandler alongside the balancing JournalEntry it posts.
/// ReferenceType/ReferenceId are an opaque, undiscriminated pointer to whatever business
/// transaction the deduction was computed against (e.g. ReferenceType="SupplierPayment",
/// ReferenceId=the ApLedgerEntry id) — same "opaque cross-concern reference, no FK" pattern
/// as ApLedgerEntry.PurchaseOrderId, except here it can point at more than one kind of thing,
/// so it carries its own discriminator string rather than being typed to one aggregate.
/// </summary>
public sealed class WithholdingTaxTransaction : TenantAggregateRoot
{
    public Guid WithholdingTaxRateId { get; private set; }
    public string ReferenceType { get; private set; } = default!;
    public Guid ReferenceId { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal DeductedAmount { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }

    private WithholdingTaxTransaction() { }

    public static WithholdingTaxTransaction Create(
        Guid companyId, Guid withholdingTaxRateId, string referenceType, Guid referenceId, decimal grossAmount, decimal deductedAmount)
    {
        if (withholdingTaxRateId == Guid.Empty)
            throw new ArgumentException("Withholding tax rate id is required.", nameof(withholdingTaxRateId));
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("Reference type is required.", nameof(referenceType));
        if (referenceId == Guid.Empty)
            throw new ArgumentException("Reference id is required.", nameof(referenceId));
        if (grossAmount <= 0)
            throw new ArgumentException("Gross amount must be positive.", nameof(grossAmount));
        if (deductedAmount <= 0)
            throw new ArgumentException("Deducted amount must be positive.", nameof(deductedAmount));

        return new WithholdingTaxTransaction
        {
            CompanyId = companyId,
            WithholdingTaxRateId = withholdingTaxRateId,
            ReferenceType = referenceType.Trim(),
            ReferenceId = referenceId,
            GrossAmount = grossAmount,
            DeductedAmount = deductedAmount,
            TransactionDate = DateTimeOffset.UtcNow,
        };
    }
}
