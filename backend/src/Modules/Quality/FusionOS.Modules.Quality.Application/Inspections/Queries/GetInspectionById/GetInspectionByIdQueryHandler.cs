using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.GetInspectionById;

public sealed class GetInspectionByIdQueryHandler : IRequestHandler<GetInspectionByIdQuery, InspectionDto>
{
    private readonly IInspectionRepository _repository;

    public GetInspectionByIdQueryHandler(IInspectionRepository repository) => _repository = repository;

    public async Task<InspectionDto> Handle(GetInspectionByIdQuery request, CancellationToken cancellationToken)
    {
        var inspection = await _repository.GetByIdAsync(request.CompanyId, request.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection '{request.InspectionId}' was not found.");

        return InspectionMapper.ToDto(inspection);
    }
}
