using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.CreateIntercompanyEliminationRule;

public sealed record CreateIntercompanyEliminationRuleCommand(
    Guid CompanyId,
    string ReceivableAccountCode,
    string PayableAccountCode,
    string Description,
    Guid? BranchId = null)
    : ICommand<IntercompanyEliminationRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.intercompany-elimination-rule.create" };
    public string EntityType => nameof(Domain.IntercompanyEliminationRules.IntercompanyEliminationRule);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Created";
}
