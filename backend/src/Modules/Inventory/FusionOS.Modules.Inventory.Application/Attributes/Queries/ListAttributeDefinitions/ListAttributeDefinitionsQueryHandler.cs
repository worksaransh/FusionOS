using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.ListAttributeDefinitions;

public sealed class ListAttributeDefinitionsQueryHandler : IRequestHandler<ListAttributeDefinitionsQuery, PagedResult<AttributeDefinitionDto>>
{
    private readonly IAttributeDefinitionRepository _repository;

    public ListAttributeDefinitionsQueryHandler(IAttributeDefinitionRepository repository) => _repository = repository;

    public async Task<PagedResult<AttributeDefinitionDto>> Handle(ListAttributeDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var definitions = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = definitions.Select(CreateAttributeDefinitionCommandHandler.MapToDto).ToList();

        return new PagedResult<AttributeDefinitionDto>(dtos, request.Page, request.PageSize, total);
    }
}
