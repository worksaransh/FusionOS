using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.TaxRates.Events;

namespace FusionOS.Modules.Finance.Domain.TaxRates;

/// <summary>
/// M8b — Finance depth: multi-jurisdiction tax engine. A named rate
/// (e.g. "GST-STANDARD" / "GST 18%", "VAT-REDUCED" / "VAT 5%") that belongs
/// to exactly one TaxJurisdiction via TaxJurisdictionId — nests under
/// TaxJurisdiction the same way Bin nests under Zone (an in-module reference
/// to another Finance aggregate, a real FK, business-key uniqueness scoped
/// to company+jurisdiction), rather than being embedded as a child entity of
/// a TaxJurisdiction aggregate. Kept as its own top-level aggregate root for
/// the same reason Bin is: simplicity, and because nothing here needs the
/// invariant-enforcement benefits of a true aggregate boundary spanning both
/// entities together.
///
/// Pure master data only — see TaxJurisdiction's own doc comment for why a
/// TaxRate is deliberately NOT yet wired onto any transactional line
/// (JournalEntryLine, SalesInvoiceLine, PurchaseOrderLine) for actual tax
/// calculation; that is a distinct, separately-scoped follow-up slice.
/// </summary>
public sealed class TaxRate : TenantAggregateRoot
{
    public Guid TaxJurisdictionId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public decimal Percentage { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// GST filing support (2026-07-22) — which GST component this rate
    /// represents (CGST/SGST for an intra-state supply, IGST for an
    /// inter-state supply), or NotApplicable for a non-GST jurisdiction
    /// (VAT, sales tax, etc.). Defaults to NotApplicable so every existing
    /// TaxRate row (created before this field existed) keeps its current
    /// meaning unchanged - this is purely additive classification metadata,
    /// nothing about Percentage/Code/Name or CalculateLineTaxQuery's existing
    /// calculation changes.
    /// </summary>
    public GstComponent GstComponent { get; private set; } = GstComponent.NotApplicable;

    private TaxRate() { }

    public static TaxRate Create(Guid companyId, Guid taxJurisdictionId, string code, string name, decimal percentage, GstComponent gstComponent = GstComponent.NotApplicable)
    {
        if (taxJurisdictionId == Guid.Empty)
            throw new ArgumentException("Tax jurisdiction id is required.", nameof(taxJurisdictionId));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Tax rate code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax rate name is required.", nameof(name));
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Tax rate percentage must be between 0 and 100.", nameof(percentage));

        var taxRate = new TaxRate
        {
            CompanyId = companyId,
            TaxJurisdictionId = taxJurisdictionId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Percentage = percentage,
            GstComponent = gstComponent,
        };

        taxRate.Raise(new TaxRateCreated(taxRate.Id, companyId, taxJurisdictionId, taxRate.Code));
        return taxRate;
    }

    /// <summary>Reclassify an existing rate's GST component (e.g. correcting CGST vs IGST after setup) - independent of UpdateDetails, since this isn't a pricing change.</summary>
    public void ReclassifyGstComponent(GstComponent gstComponent) => GstComponent = gstComponent;

    /// <summary>Covers Name and Percentage only — TaxJurisdictionId is the rate's parent FK and Code is the business key (uniqueness-checked at creation, scoped to company+jurisdiction), same immutability rule as Bin.UpdateDetails.</summary>
    public void UpdateDetails(string name, decimal percentage)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tax rate name is required.", nameof(name));
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Tax rate percentage must be between 0 and 100.", nameof(percentage));

        Name = name.Trim();
        Percentage = percentage;
    }

    public void Deactivate() => IsActive = false;
}
