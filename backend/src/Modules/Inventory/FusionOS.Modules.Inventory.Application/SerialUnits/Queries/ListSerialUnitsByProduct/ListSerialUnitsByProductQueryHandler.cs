using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Queries.ListSerialUnitsByProduct;

public sealed class ListSerialUnitsByProductQueryHandler : IRequestHandler<ListSerialUnitsByProductQuery, PagedResult<SerialUnitDto>>
{
    private readonly ISerialUnitRepository _repository;

    public ListSerialUnitsByProductQueryHandler(ISerialUnitRepository repository) => _repository = repository;

    public async Task<PagedResult<SerialUnitDto>> Handle(ListSerialUnitsByProductQuery request, CancellationToken cancellationToken)
    {
        var units = await _repository.ListByProductAsync(request.CompanyId, request.ProductId, request.Status, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByProductAsync(request.CompanyId, request.ProductId, request.Status, cancellationToken);

        var dtos = units.Select(SerialUnitMapper.ToDto).ToList();

        return new PagedResult<SerialUnitDto>(dtos, request.Page, request.PageSize, total);
    }
}
