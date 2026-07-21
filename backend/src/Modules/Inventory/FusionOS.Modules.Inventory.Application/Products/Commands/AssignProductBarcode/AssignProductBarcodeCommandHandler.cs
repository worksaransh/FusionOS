using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AssignProductBarcode;

public sealed class AssignProductBarcodeCommandHandler : IRequestHandler<AssignProductBarcodeCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignProductBarcodeCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(AssignProductBarcodeCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProductId), "Product not found."),
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            // Belt-and-braces check ahead of the DB's filtered unique index (ProductConfiguration)
            // — gives a clean 400 with a field-level message instead of surfacing a raw
            // constraint-violation error to the caller.
            var existing = await _repository.GetByBarcodeAsync(request.CompanyId, request.Barcode, cancellationToken);
            if (existing is not null && existing.Id != product.Id)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.Barcode), $"Barcode '{request.Barcode}' is already assigned to another product."),
                });
            }
        }

        product.AssignBarcode(request.Barcode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductMapper.ToDto(product);
    }
}
