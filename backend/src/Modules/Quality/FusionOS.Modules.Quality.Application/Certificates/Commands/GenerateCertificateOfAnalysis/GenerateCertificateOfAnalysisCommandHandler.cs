using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Quality.Application.Certificates.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Certificates.Commands.GenerateCertificateOfAnalysis;

public sealed class GenerateCertificateOfAnalysisCommandHandler : IRequestHandler<GenerateCertificateOfAnalysisCommand, QualityCertificateDto>
{
    private readonly IQualityCertificateRepository _certificateRepository;
    private readonly IInspectionRepository _inspectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateCertificateOfAnalysisCommandHandler(
        IQualityCertificateRepository certificateRepository, IInspectionRepository inspectionRepository, IUnitOfWork unitOfWork)
    {
        _certificateRepository = certificateRepository;
        _inspectionRepository = inspectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QualityCertificateDto> Handle(GenerateCertificateOfAnalysisCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _inspectionRepository.GetByIdAsync(request.CompanyId, request.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection '{request.InspectionId}' was not found.");

        if (inspection.Status != InspectionStatus.Passed)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.InspectionId),
                    $"A certificate of analysis/conformance can only be generated for a Passed inspection (current status: {inspection.Status})."),
            });
        }

        var existing = await _certificateRepository.GetByInspectionIdAsync(request.CompanyId, request.InspectionId, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.InspectionId),
                    $"A certificate ('{existing.CertificateNumber}') was already issued for this inspection on {existing.IssuedAt}."),
            });
        }

        var certificate = Domain.Certificates.QualityCertificate.Create(request.CompanyId, request.InspectionId, request.IssuedByUserId);

        await _certificateRepository.AddAsync(certificate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QualityCertificateMapper.ToDto(certificate, InspectionMapper.ToDto(inspection));
    }
}
