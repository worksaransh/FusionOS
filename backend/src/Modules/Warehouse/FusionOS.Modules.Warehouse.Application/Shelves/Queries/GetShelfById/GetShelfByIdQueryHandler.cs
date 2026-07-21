using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Queries.GetShelfById;

public sealed class GetShelfByIdQueryHandler : IRequestHandler<GetShelfByIdQuery, ShelfDto?>
{
    private readonly IShelfRepository _repository;

    public GetShelfByIdQueryHandler(IShelfRepository repository) => _repository = repository;

    public async Task<ShelfDto?> Handle(GetShelfByIdQuery request, CancellationToken cancellationToken)
    {
        var shelf = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (shelf is null || shelf.CompanyId != request.CompanyId)
            return null;

        return new ShelfDto(shelf.Id, shelf.RackId, shelf.Name, shelf.Code, shelf.IsActive, shelf.CreatedAt);
    }
}
