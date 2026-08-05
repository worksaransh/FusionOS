using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Settings;

/// <summary>
/// One row per company (SAP-alternative gap-closure, GST e-invoicing slice) — get-or-create
/// singleton, same pattern as Finance's own FinanceSettings. Gates whether
/// GenerateEInvoiceCommandHandler is allowed to submit an invoice to the GST Invoice
/// Registration Portal at all.
///
/// EInvoicingRequired is a plain admin-set flag, not an auto-computed "turnover exceeds
/// ₹5 crore" check — this codebase has no Annual Aggregate Turnover figure tracked anywhere
/// (that would require summing a full fiscal year of invoices across potentially multiple
/// GSTINs, a materially bigger feature than gating e-invoicing itself), and the real
/// threshold has already changed once and may change again. An admin who knows their
/// company's AATO status sets this directly, same as any other compliance toggle.
/// </summary>
public sealed class GstComplianceSettings : TenantAggregateRoot
{
    public bool EInvoicingRequired { get; private set; }

    private GstComplianceSettings() { }

    public static GstComplianceSettings CreateDefault(Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));

        return new GstComplianceSettings { CompanyId = companyId, EInvoicingRequired = false };
    }

    public void Configure(bool eInvoicingRequired)
    {
        EInvoicingRequired = eInvoicingRequired;
    }
}
