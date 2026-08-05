using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Commands.DeactivateSodConflictRule;

public sealed record DeactivateSodConflictRuleCommand(Guid CompanyId, Guid Id)
    : ICommand<SodConflictRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.sod-rule.deactivate" };
    public string EntityType => nameof(Domain.SodConflictRules.SodConflictRule);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
