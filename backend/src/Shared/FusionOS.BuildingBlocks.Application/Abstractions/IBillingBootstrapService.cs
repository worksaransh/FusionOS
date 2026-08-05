namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Cross-cutting hook Core's RegisterUserCommandHandler calls when it bootstraps a brand-new
/// company's first user, so a default trial Subscription exists before anyone can meter or bill
/// usage against that tenant (Phase 1.3). Published here (rather than in Billing.Application) so
/// Core.Application can take a dependency on the abstraction without referencing
/// Billing.Application directly — same shape as ISegregationOfDutiesService, whose doc comment
/// explains the pattern in full. The concrete implementation
/// (FusionOS.Modules.Billing.Application.Bootstrap.BillingBootstrapService) is registered once
/// in BillingModule.RegisterServices.
/// </summary>
public interface IBillingBootstrapService
{
    /// <summary>
    /// Ensures <paramref name="companyId"/> has a Subscription, creating one on a default trial
    /// Plan (seeded on first use if none exists yet) when it doesn't already have one. Safe to
    /// call more than once for the same company — a no-op if a Subscription already exists.
    /// </summary>
    Task EnsureDefaultSubscriptionAsync(Guid companyId, CancellationToken cancellationToken = default);
}
