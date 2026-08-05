namespace FusionOS.Modules.Billing.Application.Plans.Contracts;

public sealed record PlanDto(
    Guid Id,
    string Code,
    string Name,
    string Tier,
    decimal MonthlyPricePerUser,
    string IncludedModules,
    DateTimeOffset CreatedAt);
