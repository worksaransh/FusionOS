using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.CreateNonConformanceReport;

/// <summary>
/// Validates the Inspection exists for this company when one is supplied — same
/// handler-level existence-check split CreateMaintenanceRequestCommandHandler uses for
/// MaintenanceRequest.AssetId. A null InspectionId (standalone NCR) skips this check entirely.
/// </summary>
public sealed class CreateNonConformanceReportCommandHandler : IRequestHandler<CreateNonConformanceReportCommand, NonConformanceReportDto>
{
    private readonly INonConformanceReportRepository _repository;
    private readonly IInspectionRepository _inspectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNonConformanceReportCommandHandler(INonConformanceReportRepository repository, IInspectionRepository inspectionRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _inspectionRepository = inspectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<NonConformanceReportDto> Handle(CreateNonConformanceReportCommand request, CancellationToken cancellationToken)
    {
        if (request.InspectionId is Guid inspectionId)
        {
            var inspection = await _inspectionRepository.GetByIdAsync(request.CompanyId, inspectionId, cancellationToken);
            if (inspection is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.InspectionId), $"Inspection '{inspectionId}' does not exist for this company."),
                });
            }
        }

        var report = Domain.NonConformanceReports.NonConformanceReport.Create(
            request.CompanyId, request.InspectionId, request.Description, request.Severity, request.RaisedByUserId);

        await _repository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NonConformanceReportMapper.ToDto(report);
    }
}
