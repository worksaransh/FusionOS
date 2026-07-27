namespace FusionOS.Modules.Finance.Domain.TaxRates;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum GstComponent
{
    /// <summary>Not a GST rate at all (VAT, sales tax, or any other non-GST jurisdiction's rate).</summary>
    NotApplicable,

    /// <summary>Central GST — half of an intra-state supply's GST, alongside SGST.</summary>
    CGST,

    /// <summary>State GST — the other half of an intra-state supply's GST, alongside CGST.</summary>
    SGST,

    /// <summary>Integrated GST — the full rate for an inter-state supply (no CGST/SGST split).</summary>
    IGST,
}
