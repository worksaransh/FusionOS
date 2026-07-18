using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.ListInspections;

public sealed class ListInspectionsQueryHandler : IRequestHandler<ListInspectionsQuery, PagedResult<InspectionDto>>
{
    private readonly IInspectionRepository _repository;

    public ListInspectionsQueryHandler(IInspectionRepository repository) => _repository = repository;

    public async Task<PagedResult<InspectionDto>> Handle(ListInspectionsQuery request, CancellationToken cancellationToken)
    {
        var inspections = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = inspections.Select(InspectionMapper.ToDto).ToList();

        return new PagedResult<InspectionDto>(dtos, request.Page, request.PageSize, total);
    }
}
