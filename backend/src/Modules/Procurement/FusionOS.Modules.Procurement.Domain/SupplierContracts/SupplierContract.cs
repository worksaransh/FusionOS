using FusionOS.SharedKernel;
using FusionOS.Modules.Procurement.Domain.SupplierContracts.Events;

namespace FusionOS.Modules.Procurement.Domain.SupplierContracts;

/// <summary>
/// A supplier contract — validity period + terms text (docs/IMPLEMENTATION_PLAN.md
/// Phase 10 item 2's "contracts" line, alongside supplier scorecards). Deliberately
/// minimal: no pricing schedule, no auto-renewal, no line items — those would be
/// speculative scope this PRD line doesn't ask for. SupplierId is a real same-module
/// foreign key, validated the same way CreatePurchaseOrderCommandHandler validates
/// it against ISupplierRepository.ExistsAsync. Lifecycle is Active → Terminated —
/// one-way, no reactivation, matching Supplier.Deactivate()'s own one-way restraint.
/// </summary>
public sealed class SupplierContract : TenantAggregateRoot
{
    public Guid SupplierId { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public string Terms { get; private set; } = default!;
    public SupplierContractStatus Status { get; private set; }

    private SupplierContract() { }

    public static SupplierContract Create(Guid companyId, Guid supplierId, DateTimeOffset startDate, DateTimeOffset endDate, string terms)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after the start date.", nameof(endDate));
        if (string.IsNullOrWhiteSpace(terms))
            throw new ArgumentException("Terms are required.", nameof(terms));

        var contract = new SupplierContract
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            StartDate = startDate,
            EndDate = endDate,
            Terms = terms.Trim(),
            Status = SupplierContractStatus.Active,
        };

        contract.Raise(new SupplierContractCreated(contract.Id, companyId, supplierId));
        return contract;
    }

    /// <summary>One-way — no reactivation, same restraint as Supplier.Deactivate().</summary>
    public void Terminate()
    {
        if (Status == SupplierContractStatus.Terminated)
            throw new InvalidOperationException("This supplier contract has already been terminated.");

        Status = SupplierContractStatus.Terminated;
        Raise(new SupplierContractTerminated(Id, CompanyId, SupplierId));
    }
}
