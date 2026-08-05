using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Explicit, permission-gated creation path for a company's Subscription (e.g. an admin
/// switching a tenant onto a specific Plan). Distinct from BillingBootstrapService's own
/// call into Subscription.Create — that one runs unconditionally, with no permission check,
/// from inside RegisterUserCommandHandler's anonymous company-bootstrap path.
/// </summary>
public sealed record CreateSubscriptionCommand(Guid CompanyId, Guid PlanId, int? TrialDays)
    : ICommand<SubscriptionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "billing.subscription.create" };
    public string EntityType => nameof(Domain.Subscriptions.Subscription);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Created";
}
