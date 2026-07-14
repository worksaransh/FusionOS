using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Ledger.Queries.GetStockOnHand;

public sealed class GetStockOnHandQueryHandler : IRequestHandler<GetStockOnHandQuery, StockOnHandDto>
{
    private readonly IInventoryLedgerRepository _repository;

    public GetStockOnHandQueryHandler(IInventoryLedgerRepository repository) => _repository = repository;

    public async Task<StockOnHandDto> Handle(GetStockOnHandQuery request, CancellationToken cancellationToken)
    {
        var quantity = await _repository.SumQuantityAsync(request.CompanyId, request.ProductId, request.WarehouseId, cancellationToken);
        return new StockOnHandDto(request.ProductId, request.WarehouseId, quantity);
    }
}
