using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Commands.RegisterSerialUnit;

public sealed class RegisterSerialUnitCommandHandler : IRequestHandler<RegisterSerialUnitCommand, SerialUnitDto>
{
    private readonly ISerialUnitRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterSerialUnitCommandHandler(ISerialUnitRepository repository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SerialUnitDto> Handle(RegisterSerialUnitCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProductId), "Product not found."),
            });
        }

        if (await _repository.SerialNumberExistsAsync(request.CompanyId, request.ProductId, request.SerialNumber, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SerialNumber), $"Serial number '{request.SerialNumber}' already exists for this product."),
            });
        }

        var unit = Domain.SerialUnits.SerialUnit.Create(request.CompanyId, request.ProductId, request.SerialNumber);

        await _repository.AddAsync(unit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SerialUnitMapper.ToDto(unit);
    }
}
