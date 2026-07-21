using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.UpdateNonConformanceReportStatus;

public sealed class UpdateNonConformanceReportStatusCommandHandler : IRequestHandler<UpdateNonConformanceReportStatusCommand, NonConformanceReportDto>
{
    private readonly INonConformanceReportRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public UpdateNonConformanceReportStatusCommandHandler(INonConformanceReportRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<NonConformanceReportDto> Handle(UpdateNonConformanceReportStatusCommand request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(request.CompanyId, request.NonConformanceReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Non-conformance report '{request.NonConformanceReportId}' was not found.");

        report.UpdateStatus(request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NonConformanceReportMapper.ToDto(report);
    }
}
