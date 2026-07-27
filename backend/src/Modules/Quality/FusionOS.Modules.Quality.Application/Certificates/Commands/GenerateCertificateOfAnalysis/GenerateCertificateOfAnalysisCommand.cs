using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Certificates.Contracts;

namespace FusionOS.Modules.Quality.Application.Certificates.Commands.GenerateCertificateOfAnalysis;

/// <summary>
/// Generates a Certificate of Analysis/Conformance for a Passed inspection. Idempotent per
/// inspection: the handler rejects a second certificate for the same InspectionId (see
/// QualityCertificate's doc comment for why persisting, rather than purely deriving, matters
/// here). Does not generate a PDF — returns structured data (QualityCertificateDto) a
/// caller/frontend renders/prints.
/// </summary>
public sealed record GenerateCertificateOfAnalysisCommand(Guid CompanyId, Guid InspectionId, Guid IssuedByUserId)
    : ICommand<QualityCertificateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.coa.generate" };
    public string EntityType => nameof(Domain.Certificates.QualityCertificate);
    public Guid EntityId { get; init; }
    public string Action => "Generated";
}
