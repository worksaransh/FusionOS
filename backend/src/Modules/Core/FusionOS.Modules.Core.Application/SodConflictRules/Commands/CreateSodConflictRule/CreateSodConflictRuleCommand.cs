using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Commands.CreateSodConflictRule;

public sealed record CreateSodConflictRuleCommand(Guid CompanyId, string ConflictingPermissionA, string ConflictingPermissionB, string? Description)
    : ICommand<SodConflictRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.sod-rule.create" };
    public string EntityType => nameof(Domain.SodConflictRules.SodConflictRule);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Created";
}
