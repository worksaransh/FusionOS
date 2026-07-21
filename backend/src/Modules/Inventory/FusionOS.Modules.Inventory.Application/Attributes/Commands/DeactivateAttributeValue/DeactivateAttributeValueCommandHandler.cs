using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeValue;

public sealed class DeactivateAttributeValueCommandHandler : IRequestHandler<DeactivateAttributeValueCommand, AttributeValueDto>
{
    private readonly IAttributeValueRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAttributeValueCommandHandler(IAttributeValueRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttributeValueDto> Handle(DeactivateAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var attributeValue = await _repository.GetByIdAsync(request.CompanyId, request.AttributeValueId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attribute value '{request.AttributeValueId}' was not found.");

        attributeValue.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAttributeValueCommandHandler.MapToDto(attributeValue);
    }
}
