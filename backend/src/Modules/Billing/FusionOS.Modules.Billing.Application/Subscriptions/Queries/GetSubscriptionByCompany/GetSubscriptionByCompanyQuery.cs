using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Queries.GetSubscriptionByCompany;

public sealed record GetSubscriptionByCompanyQuery(Guid CompanyId) : IQuery<SubscriptionDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "billing.subscription.read" };
}
