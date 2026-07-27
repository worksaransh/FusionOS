using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Queries.ListVariantAttributeValues;

public sealed class ListVariantAttributeValuesQueryHandler : IRequestHandler<ListVariantAttributeValuesQuery, IReadOnlyList<ProductVariantAttributeValueDto>>
{
    private readonly IProductVariantAttributeValueRepository _repository;
    private readonly IAttributeDefinitionRepository _definitionRepository;
    private readonly IAttributeValueRepository _valueRepository;

    public ListVariantAttributeValuesQueryHandler(
        IProductVariantAttributeValueRepository repository,
        IAttributeDefinitionRepository definitionRepository,
        IAttributeValueRepository valueRepository)
    {
        _repository = repository;
        _definitionRepository = definitionRepository;
        _valueRepository = valueRepository;
    }

    public async Task<IReadOnlyList<ProductVariantAttributeValueDto>> Handle(ListVariantAttributeValuesQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _repository.ListForVariantAsync(request.VariantId, cancellationToken);

        var dtos = new List<ProductVariantAttributeValueDto>(assignments.Count);
        foreach (var assignment in assignments)
        {
            var definition = await _definitionRepository.GetByIdAsync(request.CompanyId, assignment.AttributeDefinitionId, cancellationToken);
            var value = await _valueRepository.GetByIdAsync(request.CompanyId, assignment.AttributeValueId, cancellationToken);

            dtos.Add(new ProductVariantAttributeValueDto(
                assignment.Id,
                assignment.ProductId,
                assignment.VariantId,
                assignment.AttributeDefinitionId,
                definition?.Name ?? string.Empty,
                assignment.AttributeValueId,
                value?.Value ?? string.Empty));
        }

        return dtos;
    }
}
