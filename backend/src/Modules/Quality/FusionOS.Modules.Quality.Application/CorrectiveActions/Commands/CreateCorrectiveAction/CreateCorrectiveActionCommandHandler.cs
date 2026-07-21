using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CreateCorrectiveAction;

/// <summary>Validates the NonConformanceReport exists for this company before creating the plan — same handler-level existence-check split CreateMaintenanceRequestCommandHandler uses for MaintenanceRequest.AssetId.</summary>
public sealed class CreateCorrectiveActionCommandHandler : IRequestHandler<CreateCorrectiveActionCommand, CorrectiveActionDto>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly INonConformanceReportRepository _nonConformanceReportRepository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public CreateCorrectiveActionCommandHandler(
        ICorrectiveActionRepository repository,
        INonConformanceReportRepository nonConformanceReportRepository,
        FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _nonConformanceReportRepository = nonConformanceReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CorrectiveActionDto> Handle(CreateCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var ncr = await _nonConformanceReportRepository.GetByIdAsync(request.CompanyId, request.NonConformanceReportId, cancellationToken);
        if (ncr is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.NonConformanceReportId), $"Non-conformance report '{request.NonConformanceReportId}' does not exist for this company."),
            });
        }

        var correctiveAction = Domain.CorrectiveActions.CorrectiveAction.Create(
            request.CompanyId,
            request.NonConformanceReportId,
            request.RootCauseDescription,
            request.CorrectiveActionDescription,
            request.PreventiveActionDescription,
            request.AssignedToUserId,
            request.DueDate);

        await _repository.AddAsync(correctiveAction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CorrectiveActionMapper.ToDto(correctiveAction);
    }
}
