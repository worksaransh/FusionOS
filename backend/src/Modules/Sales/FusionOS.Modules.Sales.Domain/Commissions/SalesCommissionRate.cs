using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Commissions;

/// <summary>
/// A commission-rate-per-salesperson (docs/IMPLEMENTATION_PLAN.md Phase 10 item
/// 11) — one row per (CompanyId, UserId), get-or-create via SetCommissionRateCommandHandler
/// (same restraint as CompanySettings' get-or-create pattern, Phase 5). UserId is
/// an opaque cross-module reference into Core's User — never existence-validated,
/// same convention as ApprovalRequest's approver ids in Phase M7, since validating
/// it would require a Sales→Core project reference this module doesn't otherwise
/// take.
/// </summary>
public sealed class SalesCommissionRate : TenantAggregateRoot
{
    public Guid UserId { get; private set; }
    public decimal RatePercentage { get; private set; }

    private SalesCommissionRate() { }

    public static SalesCommissionRate Create(Guid companyId, Guid userId, decimal ratePercentage)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));
        if (ratePercentage < 0 || ratePercentage > 100)
            throw new ArgumentException("Rate percentage must be between 0 and 100.", nameof(ratePercentage));

        return new SalesCommissionRate
        {
            CompanyId = companyId,
            UserId = userId,
            RatePercentage = ratePercentage,
        };
    }

    /// <summary>Overwrites the previous rate — "record what's true now" semantics, same as PickListLine.RecordPicked/CycleCount.RecordCount.</summary>
    public void SetRate(decimal ratePercentage)
    {
        if (ratePercentage < 0 || ratePercentage > 100)
            throw new ArgumentException("Rate percentage must be between 0 and 100.", nameof(ratePercentage));

        RatePercentage = ratePercentage;
    }
}
