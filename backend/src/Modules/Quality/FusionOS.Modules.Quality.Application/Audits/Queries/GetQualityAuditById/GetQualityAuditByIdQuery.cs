using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;

namespace FusionOS.Modules.Quality.Application.Audits.Queries.GetQualityAuditById;

public sealed record GetQualityAuditByIdQuery(Guid CompanyId, Guid QualityAuditId) : IQuery<QualityAuditDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.audit.read" };
}
