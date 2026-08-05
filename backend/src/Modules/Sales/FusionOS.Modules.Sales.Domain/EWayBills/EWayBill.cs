using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.EWayBills;

/// <summary>
/// GST e-way bill for one Dispatch (SAP-alternative gap-closure, e-way bill slice) — a
/// separate top-level aggregate rather than fields bolted onto Dispatch, since an e-way bill
/// has its own lifecycle (a real one can be extended/cancelled independently of the dispatch
/// it covers, even though this slice only covers generation). DispatchId is a real,
/// same-module foreign key — unlike Invoice's e-invoice fields, which live directly on
/// Invoice because an invoice's IRN has no independent lifecycle of its own.
///
/// One e-way bill per Dispatch (enforced by a unique index on DispatchId, see
/// EWayBillConfiguration) — GenerateEWayBillCommandHandler's idempotency check
/// (IEWayBillRepository.GetByDispatchIdAsync) is the same "already exists" guard used
/// throughout this codebase, not a database-level surprise.
/// </summary>
public sealed class EWayBill : TenantAggregateRoot
{
    public Guid DispatchId { get; private set; }
    public string EwbNumber { get; private set; } = default!;
    public DateTimeOffset GeneratedAt { get; private set; }
    public DateTimeOffset ValidUntil { get; private set; }

    private EWayBill() { }

    public static EWayBill Create(Guid companyId, Guid dispatchId, string ewbNumber, DateTimeOffset generatedAt, DateTimeOffset validUntil)
    {
        if (dispatchId == Guid.Empty)
            throw new ArgumentException("Dispatch id is required.", nameof(dispatchId));
        if (string.IsNullOrWhiteSpace(ewbNumber))
            throw new ArgumentException("E-way bill number is required.", nameof(ewbNumber));
        if (validUntil <= generatedAt)
            throw new ArgumentException("Valid-until must be after the generated-at timestamp.", nameof(validUntil));

        return new EWayBill
        {
            CompanyId = companyId,
            DispatchId = dispatchId,
            EwbNumber = ewbNumber.Trim(),
            GeneratedAt = generatedAt,
            ValidUntil = validUntil,
        };
    }
}
