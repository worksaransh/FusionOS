using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;

public sealed class CreateAttributeDefinitionCommandHandler : IRequestHandler<CreateAttributeDefinitionCommand, AttributeDefinitionDto>
{
    private readonly IAttributeDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttributeDefinitionDto> Handle(CreateAttributeDefinitionCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.NameExistsAsync(request.CompanyId, request.Name, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Name), $"Attribute '{request.Name}' already exists for this company."),
            });
        }

        var definition = AttributeDefinition.Create(request.CompanyId, request.Name);

        await _repository.AddAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(definition);
    }

    internal static AttributeDefinitionDto MapToDto(AttributeDefinition definition) => new(
        definition.Id, definition.Name, definition.IsActive, definition.CreatedAt);
}
