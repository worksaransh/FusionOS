using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeDefinition;

public sealed class DeactivateAttributeDefinitionCommandHandler : IRequestHandler<DeactivateAttributeDefinitionCommand, AttributeDefinitionDto>
{
    private readonly IAttributeDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttributeDefinitionDto> Handle(DeactivateAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetByIdAsync(request.CompanyId, request.AttributeDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attribute definition '{request.AttributeDefinitionId}' was not found.");

        definition.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAttributeDefinitionCommandHandler.MapToDto(definition);
    }
}
