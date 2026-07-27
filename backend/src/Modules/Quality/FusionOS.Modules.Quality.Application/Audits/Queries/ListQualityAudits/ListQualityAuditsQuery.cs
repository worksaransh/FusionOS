using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;

namespace FusionOS.Modules.Quality.Application.Audits.Queries.ListQualityAudits;

public sealed record ListQualityAuditsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<QualityAuditDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.audit.read" };
}
