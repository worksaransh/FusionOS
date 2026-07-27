using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Certificates.Contracts;

namespace FusionOS.Modules.Quality.Application.Certificates.Queries.GetCertificateByInspectionId;

public sealed record GetCertificateByInspectionIdQuery(Guid CompanyId, Guid InspectionId) : IQuery<QualityCertificateDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.coa.read" };
}
