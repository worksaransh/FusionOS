using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Certificates.Contracts;

namespace FusionOS.Modules.Quality.Application.Certificates.Queries.ListCertificates;

public sealed record ListCertificatesQuery(Guid CompanyId, int Page, int PageSize) : IQuery<PagedResult<QualityCertificateSummaryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.coa.read" };
}
