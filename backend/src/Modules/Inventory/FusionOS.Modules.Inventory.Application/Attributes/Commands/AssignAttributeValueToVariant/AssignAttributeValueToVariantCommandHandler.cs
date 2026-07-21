using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.AssignAttributeValueToVariant;

public sealed class AssignAttributeValueToVariantCommandHandler : IRequestHandler<AssignAttributeValueToVariantCommand, ProductVariantAttributeValueDto>
{
    private readonly IProductVariantAttributeValueRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IAttributeValueRepository _attributeValueRepository;
    private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignAttributeValueToVariantCommandHandler(
        IProductVariantAttributeValueRepository repository,
        IProductRepository productRepository,
        IAttributeValueRepository attributeValueRepository,
        IAttributeDefinitionRepository attributeDefinitionRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _productRepository = productRepository;
        _attributeValueRepository = attributeValueRepository;
        _attributeDefinitionRepository = attributeDefinitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductVariantAttributeValueDto> Handle(AssignAttributeValueToVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProductId), "Product not found."),
            });
        }

        // Queried directly rather than via product.Variants — see
        // IProductRepository.VariantExistsAsync's doc comment.
        if (!await _productRepository.VariantExistsAsync(request.ProductId, request.VariantId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.VariantId), "Variant not found on this product."),
            });
        }

        var attributeValue = await _attributeValueRepository.GetByIdAsync(request.CompanyId, request.AttributeValueId, cancellationToken);
        if (attributeValue is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AttributeValueId), "Attribute value not found."),
            });
        }

        var definition = await _attributeDefinitionRepository.GetByIdAsync(request.CompanyId, attributeValue.AttributeDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attribute definition '{attributeValue.AttributeDefinitionId}' was not found.");

        // Upsert — see ProductVariantAttributeValue's doc comment: one value per definition per variant.
        var existing = await _repository.GetForVariantAndDefinitionAsync(request.VariantId, definition.Id, cancellationToken);
        if (existing is not null)
            _repository.Remove(existing);

        var assignment = ProductVariantAttributeValue.Create(request.CompanyId, request.ProductId, request.VariantId, definition.Id, attributeValue.Id);
        await _repository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductVariantAttributeValueDto(
            assignment.Id, assignment.ProductId, assignment.VariantId, assignment.AttributeDefinitionId, definition.Name, assignment.AttributeValueId, attributeValue.Value);
    }
}
