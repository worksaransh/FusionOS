using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;

public sealed class RecordInspectionResultsCommandHandler : IRequestHandler<RecordInspectionResultsCommand, InspectionDto>
{
    private readonly IInspectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordInspectionResultsCommandHandler(IInspectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InspectionDto> Handle(RecordInspectionResultsCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(request.CompanyId, request.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection '{request.InspectionId}' was not found.");

        // Resolves the inspection to Passed/Failed and raises InspectionCompleted.
        inspection.RecordResults(request.Results);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InspectionMapper.ToDto(inspection);
    }
}
