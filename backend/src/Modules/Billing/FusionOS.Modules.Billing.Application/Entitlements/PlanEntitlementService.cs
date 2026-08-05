using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Domain.Subscriptions;
using Microsoft.Extensions.Logging;

namespace FusionOS.Modules.Billing.Application.Entitlements;

/// <summary>
/// Implements IPlanEntitlementService (FusionOS.BuildingBlocks.Application.Abstractions) —
/// called by PlanEntitlementBehavior on (almost) every request, the same "cross-module service
/// called from inside another module's pipeline" shape as ISegregationOfDutiesService.
///
/// <b>Fail-open is the load-bearing behavior here, not an afterthought:</b>
///  - No Subscription row for the company at all -&gt; true. This is the common case for every
///    company that existed before the Billing module did, and for every integration test that
///    seeds a Company directly through ICompanyRepository (FusionOSWebApplicationFactory
///    .SeedCompanyWithOwnerAsync) rather than through the real registration flow that calls
///    IBillingBootstrapService. Failing closed here would 403 every one of those companies out
///    of their own ERP.
///  - Subscription's Plan row is somehow missing (data integrity edge case) -&gt; true, for the
///    same reason.
///  - Anything else goes wrong reaching Billing's data — most notably the BillingDbContext's
///    tables not existing at all in a given database (FusionOSWebApplicationFactory only calls
///    EnsureCreatedAsync for CoreDbContext/CrmDbContext, never BillingDbContext, so any query
///    here against a test database throws a Postgres "relation does not exist" error) -&gt; true.
///    Caught broadly and deliberately: this check must never be the reason a request fails when
///    the actual failure is "Billing's schema isn't provisioned here", which has nothing to do
///    with whether the company is entitled to the module.
///
/// Only the one narrow path blocks: a Subscription exists, its Status is Active or Trial, and
/// its Plan's IncludedModules explicitly does not list the module. PastDue/Cancelled
/// subscriptions are deliberately NOT blocked here — dunning/suspension is a distinct concern
/// from plan-module entitlement and isn't in scope for this check.
/// </summary>
public sealed class PlanEntitlementService : IPlanEntitlementService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<PlanEntitlementService> _logger;

    public PlanEntitlementService(
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        ILogger<PlanEntitlementService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<bool> IsModuleIncludedAsync(Guid companyId, string moduleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByCompanyIdAsync(companyId, cancellationToken);
            if (subscription is null)
                return true; // Fail-open: no Subscription row at all — see class doc comment.

            if (subscription.Status != SubscriptionStatus.Active && subscription.Status != SubscriptionStatus.Trial)
                return true; // Out of scope for this check — see class doc comment.

            var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
            if (plan is null)
                return true; // Fail-open: dangling PlanId — see class doc comment.

            var includedModules = plan.IncludedModules.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return includedModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            // Fail-open: Billing's own tables/data being unreachable (e.g. schema not
            // provisioned in a given environment/test database) must never itself deny access
            // to another module — see class doc comment.
            _logger.LogWarning(ex,
                "PlanEntitlementService failed to resolve entitlement for company {CompanyId}/module {ModuleName} — failing open.",
                companyId, moduleName);
            return true;
        }
    }
}
