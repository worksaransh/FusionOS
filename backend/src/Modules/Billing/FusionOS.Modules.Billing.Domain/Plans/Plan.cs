using FusionOS.SharedKernel;

namespace FusionOS.Modules.Billing.Domain.Plans;

/// <summary>
/// A FusionOS pricing plan (Starter/Growth/Enterprise) — global catalog data, not scoped to
/// any one tenant, same shape/lifecycle as Company itself: audited and soft-deletable but
/// deliberately NOT a TenantAggregateRoot (there is no "owning company" for a plan every
/// company can subscribe to). Modeled after Company (FusionOS.Modules.Core.Domain.Companies)
/// rather than a tenant-scoped catalog entity like TaxJurisdiction, which is scoped per-company
/// because two companies can define differently-named jurisdictions; a Plan's Code/Name/pricing
/// is the same for every prospective subscriber.
///
/// IncludedModules is stored as a plain comma-separated string column rather than an EF owned
/// collection/join table — this is catalog metadata read back as a whole for display, never
/// queried element-by-element, so a delimited string is the simplest representation that still
/// round-trips cleanly (same "opaque string, no relational modeling" choice this codebase makes
/// elsewhere for a value that's read as a unit, e.g. WithholdingTaxTransaction.ReferenceType).
/// </summary>
public sealed class Plan : AuditableAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public PlanTier Tier { get; private set; }
    public decimal MonthlyPricePerUser { get; private set; }
    public string IncludedModules { get; private set; } = default!;

    private Plan() { }

    public static Plan Create(string code, string name, PlanTier tier, decimal monthlyPricePerUser, string includedModules)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Plan code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required.", nameof(name));
        if (monthlyPricePerUser < 0)
            throw new ArgumentException("Monthly price per user cannot be negative.", nameof(monthlyPricePerUser));

        return new Plan
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Tier = tier,
            MonthlyPricePerUser = monthlyPricePerUser,
            IncludedModules = (includedModules ?? string.Empty).Trim(),
        };
    }
}
