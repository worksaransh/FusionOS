using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Plans.Contracts;

namespace FusionOS.Modules.Billing.Application.Plans.Commands.CreatePlan;

/// <summary>
/// Tier travels as a string over the wire and is parsed against PlanTier in the handler —
/// same convention as FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate
/// .CreateTaxRateCommand carries GstComponent as a string.
/// </summary>
public sealed record CreatePlanCommand(string Code, string Name, string Tier, decimal MonthlyPricePerUser, string IncludedModules)
    : ICommand<PlanDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "billing.plan.create" };
    public string EntityType => nameof(Domain.Plans.Plan);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Created";
}
