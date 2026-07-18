using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions.Events;

namespace FusionOS.Modules.Finance.Domain.TaxJurisdictions;

/// <summary>
/// M8b — Finance depth: multi-jurisdiction tax engine. Pure master data
/// (Code/Name/IsActive), same shape and lifecycle as CostCenter (M8a) — a
/// TaxJurisdiction represents a taxing authority's scope (country, or a
/// state/province within one, e.g. "IN-KA" for Karnataka/India, "US-CA" for
/// California/USA, or a company-wide "DEFAULT") that one or more named
/// TaxRate children reference by TaxJurisdictionId. This resolves the
/// tax-jurisdiction design decision (PROJECT_TRACKER.md Section 4 item 2,
/// resolved 2026-07-16) as "multi-jurisdiction/multi-rate": each jurisdiction
/// owns its own set of independently-named rates (see TaxRate), not one flat
/// global tax percentage.
///
/// Like CostCenter, this slice only builds the master-data aggregates
/// themselves — a TaxRate is deliberately NOT yet wired onto
/// JournalEntryLine, SalesInvoiceLine, or PurchaseOrderLine for line-level
/// tax calculation. Attaching an optional TaxRateId onto those lines (and
/// the actual amount-calculation logic that would use it) is a natural,
/// separately-scoped follow-up once this master data exists to reference.
/// </summary>
public sealed class TaxJurisdiction : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private TaxJurisdiction() { }

    public static TaxJurisdiction Create(Guid companyId, string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Tax jurisdiction code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax jurisdiction name is required.", nameof(name));

        var jurisdiction = new TaxJurisdiction
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
        };

        jurisdiction.Raise(new TaxJurisdictionCreated(jurisdiction.Id, companyId, jurisdiction.Code));
        return jurisdiction;
    }

    /// <summary>Updates the mutable master-data field. Code and CompanyId are the tenant-scoped business key and stay immutable after creation.</summary>
    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax jurisdiction name is required.", nameof(name));

        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
}
