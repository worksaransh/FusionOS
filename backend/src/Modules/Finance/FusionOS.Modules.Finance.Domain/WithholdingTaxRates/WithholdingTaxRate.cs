using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.WithholdingTaxRates;

/// <summary>
/// TDS (Tax Deducted at Source) / TCS (Tax Collected at Source) rate under the Income Tax
/// Act — a genuinely distinct regime from GST TDS/TCS (TaxRate.GstComponent only models
/// CGST/SGST/IGST) and from GST itself: SectionCode is an Income Tax Act section (e.g.
/// "194C" — payments to contractors, "194J" — professional fees, "206C(1H)" — TCS on sale of
/// goods), not a GST jurisdiction/HSN code. Modeled separately from TaxRate rather than
/// extended onto it, per this slice's own scope decision (SAP-alternative gap-closure).
/// </summary>
public sealed class WithholdingTaxRate : TenantAggregateRoot
{
    public string SectionCode { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Percentage { get; private set; }
    public bool IsActive { get; private set; }

    private WithholdingTaxRate() { }

    public static WithholdingTaxRate Create(Guid companyId, string sectionCode, string description, decimal percentage)
    {
        if (string.IsNullOrWhiteSpace(sectionCode))
            throw new ArgumentException("Section code is required.", nameof(sectionCode));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100.", nameof(percentage));

        return new WithholdingTaxRate
        {
            CompanyId = companyId,
            SectionCode = sectionCode.Trim(),
            Description = description.Trim(),
            Percentage = percentage,
            IsActive = true,
        };
    }

    public void Deactivate() => IsActive = false;
}
