using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Queries.GetWarehouseById;

public sealed class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{
    private readonly IWarehouseRepository _repository;

    public GetWarehouseByIdQueryHandler(IWarehouseRepository repository) => _repository = repository;

    public async Task<WarehouseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse is null || warehouse.CompanyId != request.CompanyId)
            return null;

        return new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Code, warehouse.Address, warehouse.IsActive, warehouse.CreatedAt);
    }
}
