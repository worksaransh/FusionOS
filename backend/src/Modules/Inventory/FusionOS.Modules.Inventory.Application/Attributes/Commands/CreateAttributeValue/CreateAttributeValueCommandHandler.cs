using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;

public sealed class CreateAttributeValueCommandHandler : IRequestHandler<CreateAttributeValueCommand, AttributeValueDto>
{
    private readonly IAttributeValueRepository _repository;
    private readonly IAttributeDefinitionRepository _definitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAttributeValueCommandHandler(
        IAttributeValueRepository repository,
        IAttributeDefinitionRepository definitionRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _definitionRepository = definitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttributeValueDto> Handle(CreateAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var definition = await _definitionRepository.GetByIdAsync(request.CompanyId, request.AttributeDefinitionId, cancellationToken);
        if (definition is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AttributeDefinitionId), "Attribute definition not found."),
            });
        }

        if (await _repository.ValueExistsAsync(request.AttributeDefinitionId, request.Value, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Value), $"Value '{request.Value}' already exists for attribute '{definition.Name}'."),
            });
        }

        var attributeValue = AttributeValue.Create(request.CompanyId, request.AttributeDefinitionId, request.Value);

        await _repository.AddAsync(attributeValue, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(attributeValue);
    }

    internal static AttributeValueDto MapToDto(AttributeValue attributeValue) => new(
        attributeValue.Id, attributeValue.AttributeDefinitionId, attributeValue.Value, attributeValue.IsActive, attributeValue.CreatedAt);
}
