using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid CompanyId, Guid SubscriptionId)
    : ICommand<SubscriptionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "billing.subscription.cancel" };
    public string EntityType => nameof(Domain.Subscriptions.Subscription);
    public Guid EntityId => SubscriptionId;
    public string Action => "Cancelled";
}
