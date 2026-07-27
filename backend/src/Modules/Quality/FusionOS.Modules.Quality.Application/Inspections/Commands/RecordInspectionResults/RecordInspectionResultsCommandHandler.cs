using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;

public sealed class RecordInspectionResultsCommandHandler : IRequestHandler<RecordInspectionResultsCommand, InspectionDto>
{
    private readonly IInspectionRepository _repository;
    private readonly IQualityHoldRepository _qualityHoldRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordInspectionResultsCommandHandler(IInspectionRepository repository, IQualityHoldRepository qualityHoldRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _qualityHoldRepository = qualityHoldRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InspectionDto> Handle(RecordInspectionResultsCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(request.CompanyId, request.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection '{request.InspectionId}' was not found.");

        // Resolves the inspection to Passed/Failed and raises InspectionCompleted.
        inspection.RecordResults(request.Results);

        // Deliberately opt-in, never automatic (see this command's own doc comment and
        // InspectionCompleted's) — only when the caller explicitly asks for it AND the
        // inspection actually failed do we create a QualityHold, targeting this inspection's
        // own ReferenceId under the caller-supplied HeldEntityType.
        if (request.RaiseHoldOnFailure && inspection.Status == InspectionStatus.Failed)
        {
            var hold = Domain.QualityHolds.QualityHold.Create(
                request.CompanyId,
                request.HeldEntityType!,
                inspection.ReferenceId,
                $"Failed {inspection.Type} inspection {inspection.Id}.",
                inspection.Id);

            await _qualityHoldRepository.AddAsync(hold, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InspectionMapper.ToDto(inspection);
    }
}
