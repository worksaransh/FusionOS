using FusionOS.Modules.Quality.Application.Certificates.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Certificates.Queries.GetCertificateByInspectionId;

public sealed class GetCertificateByInspectionIdQueryHandler : IRequestHandler<GetCertificateByInspectionIdQuery, QualityCertificateDto>
{
    private readonly IQualityCertificateRepository _certificateRepository;
    private readonly IInspectionRepository _inspectionRepository;

    public GetCertificateByInspectionIdQueryHandler(IQualityCertificateRepository certificateRepository, IInspectionRepository inspectionRepository)
    {
        _certificateRepository = certificateRepository;
        _inspectionRepository = inspectionRepository;
    }

    public async Task<QualityCertificateDto> Handle(GetCertificateByInspectionIdQuery request, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByInspectionIdAsync(request.CompanyId, request.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"No certificate has been issued for inspection '{request.InspectionId}'.");

        var inspection = await _inspectionRepository.GetByIdAsync(request.CompanyId, request.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection '{request.InspectionId}' was not found.");

        return QualityCertificateMapper.ToDto(certificate, InspectionMapper.ToDto(inspection));
    }
}
