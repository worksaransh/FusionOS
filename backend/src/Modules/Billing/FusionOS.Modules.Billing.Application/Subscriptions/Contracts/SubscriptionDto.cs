namespace FusionOS.Modules.Billing.Application.Subscriptions.Contracts;

public sealed record SubscriptionDto(
    Guid Id,
    Guid CompanyId,
    Guid PlanId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? TrialEndsAt,
    DateTimeOffset? CancelledAt);
