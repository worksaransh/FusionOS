using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Racks.Queries.ListRacks;

public sealed class ListRacksQueryHandler : IRequestHandler<ListRacksQuery, PagedResult<RackDto>>
{
    private readonly IRackRepository _repository;

    public ListRacksQueryHandler(IRackRepository repository) => _repository = repository;

    public async Task<PagedResult<RackDto>> Handle(ListRacksQuery request, CancellationToken cancellationToken)
    {
        var racks = await _repository.ListAsync(request.CompanyId, request.ZoneId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ZoneId, cancellationToken);

        var dtos = racks.Select(r => new RackDto(r.Id, r.ZoneId, r.Name, r.Code, r.IsActive, r.CreatedAt)).ToList();

        return new PagedResult<RackDto>(dtos, request.Page, request.PageSize, total);
    }
}
