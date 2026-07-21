using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.GetAttributeDefinitionById;

public sealed class GetAttributeDefinitionByIdQueryHandler : IRequestHandler<GetAttributeDefinitionByIdQuery, AttributeDefinitionDto>
{
    private readonly IAttributeDefinitionRepository _repository;

    public GetAttributeDefinitionByIdQueryHandler(IAttributeDefinitionRepository repository) => _repository = repository;

    public async Task<AttributeDefinitionDto> Handle(GetAttributeDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetByIdAsync(request.CompanyId, request.AttributeDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attribute definition '{request.AttributeDefinitionId}' was not found.");

        return CreateAttributeDefinitionCommandHandler.MapToDto(definition);
    }
}
