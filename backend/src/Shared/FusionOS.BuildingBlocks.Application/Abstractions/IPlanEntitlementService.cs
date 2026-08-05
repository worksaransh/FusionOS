namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Cross-cutting plan-entitlement lookup, published here (rather than in Billing.Application)
/// so PlanEntitlementBehavior (FusionOS.BuildingBlocks.Application.Behaviors) can depend on the
/// abstraction without BuildingBlocks.Application referencing Billing.Application directly —
/// same shape as IBillingBootstrapService/ISegregationOfDutiesService, whose doc comments
/// explain the pattern in full. The concrete implementation
/// (FusionOS.Modules.Billing.Application.Entitlements.PlanEntitlementService) is registered
/// once in BillingModule.RegisterServices.
///
/// <b>Fail-open by design, not a bug:</b> this codebase has real companies (seeded directly
/// through repositories in tests, and any company created before the Billing module existed)
/// with no Subscription row at all, and integration tests that spin up only a subset of module
/// DbContexts (so Billing's own tables may not even exist in a given test database). A missing
/// Subscription, a missing Plan, or any failure reaching Billing's data (e.g. "relation does not
/// exist") must therefore return true — "the module is included" — never false. The only way
/// this ever returns false is the narrow, deliberate case: a Subscription genuinely exists, is
/// Active or Trial, and its Plan's IncludedModules explicitly omits the module being checked.
/// PastDue/Cancelled subscriptions are intentionally out of scope for blocking here (dunning/
/// cancellation flows are a separate concern) — see PlanEntitlementService's own doc comment.
/// </summary>
public interface IPlanEntitlementService
{
    /// <summary>
    /// True unless <paramref name="companyId"/> has a genuinely Active/Trial Subscription whose
    /// Plan's IncludedModules explicitly excludes <paramref name="moduleName"/>. Fails open (returns
    /// true) on a missing Subscription, a missing Plan, or any error reaching Billing's data.
    /// </summary>
    Task<bool> IsModuleIncludedAsync(Guid companyId, string moduleName, CancellationToken cancellationToken = default);
}
