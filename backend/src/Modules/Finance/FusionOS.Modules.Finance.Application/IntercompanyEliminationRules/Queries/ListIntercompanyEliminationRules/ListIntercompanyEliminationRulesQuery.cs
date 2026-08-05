using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Queries.ListIntercompanyEliminationRules;

public sealed record ListIntercompanyEliminationRulesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<IntercompanyEliminationRuleDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.intercompany-elimination-rule.read" };
}
