using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeValuesByDefinition;

public sealed class ListAttributeValuesByDefinitionQueryHandler : IRequestHandler<ListAttributeValuesByDefinitionQuery, IReadOnlyList<AttributeValueDto>>
{
    private readonly IAttributeValueRepository _repository;

    public ListAttributeValuesByDefinitionQueryHandler(IAttributeValueRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<AttributeValueDto>> Handle(ListAttributeValuesByDefinitionQuery request, CancellationToken cancellationToken)
    {
        var values = await _repository.ListByDefinitionAsync(request.CompanyId, request.AttributeDefinitionId, cancellationToken);

        return values.Select(CreateAttributeValueCommandHandler.MapToDto).ToList();
    }
}
