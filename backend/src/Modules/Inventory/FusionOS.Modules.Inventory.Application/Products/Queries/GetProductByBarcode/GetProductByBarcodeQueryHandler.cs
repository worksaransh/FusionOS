using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Products.Queries.GetProductByBarcode;

public sealed class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByBarcodeQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<ProductDto?> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Barcode))
            return null;

        var product = await _repository.GetByBarcodeAsync(request.CompanyId, request.Barcode, cancellationToken);
        return product is null ? null : ProductMapper.ToDto(product);
    }
}
