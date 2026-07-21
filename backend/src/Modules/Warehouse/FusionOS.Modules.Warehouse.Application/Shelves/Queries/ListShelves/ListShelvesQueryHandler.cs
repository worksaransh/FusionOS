using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Queries.ListShelves;

public sealed class ListShelvesQueryHandler : IRequestHandler<ListShelvesQuery, PagedResult<ShelfDto>>
{
    private readonly IShelfRepository _repository;

    public ListShelvesQueryHandler(IShelfRepository repository) => _repository = repository;

    public async Task<PagedResult<ShelfDto>> Handle(ListShelvesQuery request, CancellationToken cancellationToken)
    {
        var shelves = await _repository.ListAsync(request.CompanyId, request.RackId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.RackId, cancellationToken);

        var dtos = shelves.Select(s => new ShelfDto(s.Id, s.RackId, s.Name, s.Code, s.IsActive, s.CreatedAt)).ToList();

        return new PagedResult<ShelfDto>(dtos, request.Page, request.PageSize, total);
    }
}
