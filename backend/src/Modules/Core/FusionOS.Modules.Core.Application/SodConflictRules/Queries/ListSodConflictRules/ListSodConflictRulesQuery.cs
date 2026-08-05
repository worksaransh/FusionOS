using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Queries.ListSodConflictRules;

public sealed record ListSodConflictRulesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SodConflictRuleDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.sod-rule.read" };
}
