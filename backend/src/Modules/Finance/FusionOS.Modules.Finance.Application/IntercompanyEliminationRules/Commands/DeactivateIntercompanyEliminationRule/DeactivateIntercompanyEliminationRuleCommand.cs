using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.DeactivateIntercompanyEliminationRule;

public sealed record DeactivateIntercompanyEliminationRuleCommand(Guid CompanyId, Guid Id)
    : ICommand<IntercompanyEliminationRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.intercompany-elimination-rule.deactivate" };
    public string EntityType => nameof(Domain.IntercompanyEliminationRules.IntercompanyEliminationRule);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
